using Billing.Application.Abstractions.Database;
using Billing.Application.Webhooks.ProcessCheckoutSessionCompleted;
using Billing.Application.Webhooks.ProcessSubscriptionDeleted;
using Billing.Application.Webhooks.ProcessSubscriptionUpdated;
using Billing.Domain.Aggregate.Customer;
using Billing.Domain.Aggregate.Webhook;
using Billing.Infrastructure.Settings;
using Billing.Infrastructure.Webhooks.Services;
using Billing.Infrastructure.Webhooks.ViewModels;
using Billing.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Billing.Infrastructure.Webhooks;

public sealed class StripeWebhookHandler
(
    IOptions<StripeSettings> options,
    IBillingDbContext context,
    IDateTimeProvider dateTimeProvider,
    ISender sender,
    IStripeSubscriptionFetcher stripeSubscriptionFetcher,
    IStripeWebhookMapper stripeWebhookMapper,
    ILogger<StripeWebhookHandler> logger
)
{
    public async Task<Result> HandleAsync(string json, string signature, CancellationToken cancellationToken)
    {
        Result<Event> verificationResult = VerifySignature(json, signature);

        if (verificationResult.IsFailure)
        {
            logger.LogWarning("Webhook signature verification failed: {Error}", verificationResult.Error.Code);
            return Result.Failure(verificationResult.Error);
        }

        Event stripeEvent = verificationResult.Value;
        
        logger.LogInformation("Received Stripe webhook event {EventId} of type {EventType}", stripeEvent.Id, stripeEvent.Type);
        
        bool alreadyProcessed = await context.WebhookEvents
            .AnyAsync(w => w.StripeEventId == stripeEvent.Id, cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation("Stripe webhook event {EventId} has already been processed", stripeEvent.Id);
            return Result.Success();
        }

        Result<WebhookEvent> webhookEventResult = WebhookEvent.Create(stripeEvent.Id, stripeEvent.Type, dateTimeProvider);
        
        if (webhookEventResult.IsFailure)
        {
            logger.LogError("Failed to create WebhookEvent entity: {Error}", webhookEventResult.Error.Code);
            return Result.Failure(webhookEventResult.Error);
        }
        
        WebhookEvent webhookEvent = webhookEventResult.Value;
                
        Result processingResult = stripeEvent.Type switch
        {
            EventTypes.CheckoutSessionCompleted => await HandleCheckoutSessionCompletedAsync(stripeEvent, cancellationToken),
            EventTypes.CustomerSubscriptionUpdated => await HandleCustomerSubscriptionUpdatedAsync(stripeEvent, cancellationToken),
            EventTypes.CustomerSubscriptionDeleted => await HandleSubscriptionDeletedAsync(stripeEvent, cancellationToken),
            _ => HandleUnsupportedEventType(stripeEvent.Type)
        };

        if (processingResult.IsSuccess)
        {
            Result markingResult = webhookEvent.MarkAsProcessed(dateTimeProvider);
            
            if (markingResult.IsFailure)
            {
                logger.LogError("Failed to mark WebhookEvent as processed: {Error}", markingResult.Error.Code);
            }
        }
        else
        {
            Result markingResult = webhookEvent.MarkAsFailed(processingResult.Error.Description, dateTimeProvider);
            
            if (markingResult.IsFailure)
            {
                logger.LogError("Failed to mark WebhookEvent as failed: {Error}", markingResult.Error.Code);
            }
        }

        await context.WebhookEvents.AddAsync(webhookEvent, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);

        return processingResult;
    }
    
    private async Task<Result> HandleCheckoutSessionCompletedAsync(Event stripeEvent,
        CancellationToken cancellationToken)
    {
        Session? session = stripeEvent.Data.Object as Session;

        if (session is null || session.CustomerId is null || session.SubscriptionId is null ||
            string.IsNullOrWhiteSpace(session.CustomerId) || string.IsNullOrWhiteSpace(session.SubscriptionId)) 
        {
            logger.LogWarning("Checkout Session object is invalid or missing required fields");
            return Result.Failure(WebhookErrors.InvalidEventData);
        }
        
        logger.LogInformation("Processing Checkout Session Completed for Session ID: {SessionId}", session.Id);
        
        Result<Subscription> subscriptionResult = await stripeSubscriptionFetcher
            .FetchWithDetailsAsync(session.SubscriptionId, cancellationToken);

        if (subscriptionResult.IsFailure)
        {
            logger.LogError("Failed to fetch subscription details from Stripe: {Error}", subscriptionResult.Error.Code);
            return Result.Failure(subscriptionResult.Error);
        }
        
        Subscription subscription = subscriptionResult.Value;
        
        SubscriptionProductInfoViewModel productInfoViewModel = stripeWebhookMapper.ExtractProductInfo(subscription);
        
        var command = new ProcessCheckoutSessionCompletedCommand
        (
            session.CustomerId,
            subscription.Id,
            productInfoViewModel.PriceId,
            productInfoViewModel.ProductName,
            productInfoViewModel.PriceFormatted,
            stripeEvent.Type,
            productInfoViewModel.CurrentPeriodStart,
            productInfoViewModel.CurrentPeriodEnd
        );
        
        return await sender.Send(command, cancellationToken);
    }
    
    private async Task<Result> HandleCustomerSubscriptionUpdatedAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        Subscription? subscription = stripeEvent.Data.Object as Subscription;
        
        if (subscription is null || subscription.CustomerId is null || string.IsNullOrWhiteSpace(subscription.CustomerId))
        {
            logger.LogWarning("Subscription object is invalid or missing required fields");
            return Result.Failure(WebhookErrors.InvalidEventData);
        }
        
        logger.LogInformation("Processing Customer Subscription Updated for Subscription ID: {SubscriptionId}", subscription.Id);
        
        SubscriptionProductInfoViewModel productInfoViewModel = stripeWebhookMapper.ExtractProductInfo(subscription);

        SubscriptionStatus mappedStatus = stripeWebhookMapper.MapStripeSubscriptionStatus(subscription.Status);
        
        var command = new ProcessSubscriptionUpdatedCommand
        (
            subscription.CustomerId,
            subscription.Id,
            mappedStatus,
            productInfoViewModel.CurrentPeriodStart,
            productInfoViewModel.CurrentPeriodEnd,
            subscription.CancelAtPeriodEnd
        );
        
        return await sender.Send(command, cancellationToken);
    }
    
    private async Task<Result> HandleSubscriptionDeletedAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        Subscription? subscription = stripeEvent.Data.Object as Subscription;
        
        if (subscription is null || subscription.CustomerId is null || string.IsNullOrWhiteSpace(subscription.CustomerId))
        {
            logger.LogWarning("Subscription object is invalid or missing required fields");
            return Result.Failure(WebhookErrors.InvalidEventData);
        }
        
        logger.LogInformation("Processing Customer Subscription Deleted for Subscription ID: {SubscriptionId}", subscription.Id);
        
        SubscriptionProductInfoViewModel productInfoViewModel = stripeWebhookMapper.ExtractProductInfo(subscription);
        
        var command = new ProcessSubscriptionDeletedCommand
        (
            subscription.CustomerId,
            subscription.Id,
            productInfoViewModel.ProductName,
            productInfoViewModel.PriceFormatted,
            productInfoViewModel.CurrentPeriodStart,
            productInfoViewModel.CurrentPeriodEnd
        );
        
        return await sender.Send(command, cancellationToken);
    }
    
    private Result HandleUnsupportedEventType(string eventType)
    {
        logger.LogInformation("Ignoring unsupported event type: {EventType}", eventType);
        
        return Result.Success();
    }
    
    private Result<Event> VerifySignature(string json, string signature)
    {
        try
        {
            Event stripeEvent = EventUtility.ConstructEvent
            (
                json,
                signature,
                options.Value.WebhookSecret,
                throwOnApiVersionMismatch: false
            );

            return Result.Success(stripeEvent);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe signature verification failed");
            return Result.Failure<Event>(WebhookErrors.InvalidSignature);
        }
    }
}
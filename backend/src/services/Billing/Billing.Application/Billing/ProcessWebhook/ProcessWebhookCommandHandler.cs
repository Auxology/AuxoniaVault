using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Abstractions.Services;
using Billing.Domain.Aggregate.Customer;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Billing.Application.Billing.ProcessWebhook;

internal sealed class ProcessWebhookCommandHandler(
    IStripeWebhookService webhookService, 
    IBillingDbContext context, 
    IDateTimeProvider dateTimeProvider,
    SubscriptionService subscriptionService,
    ProductService productService,
    PriceService priceService) : ICommandHandler<ProcessWebhookCommand>
{
    public async Task<Result> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        Result<Event> eventResult = await webhookService.ConstructEventAsync(request.EventJson, request.Signature, cancellationToken);
        
        if (eventResult.IsFailure)
            return Result.Failure(eventResult.Error);
        
        var stripeEvent = eventResult.Value;
        
        return stripeEvent.Type switch
        {
            EventTypes.CheckoutSessionCompleted => await HandleSessionCompletion(stripeEvent, cancellationToken),
            EventTypes.SubscriptionScheduleCanceled => await HandleSubscriptionUpdate(stripeEvent, cancellationToken),
            EventTypes.CustomerSubscriptionDeleted => await HandleSubscriptionDeleted(stripeEvent, cancellationToken),
            _ => Result.Success()
        };
    }

    private async Task<Result> HandleSessionCompletion(Event stripeEvent, CancellationToken cancellationToken)
    {
            var session = stripeEvent.Data.Object as Session;
        
            if (session is null)
                return Result.Success();
        
            var customer = await context.Customers
                .Include(c => c.Subscriptions)
                .FirstOrDefaultAsync(c => c.StripeCustomerId == session.CustomerId, cancellationToken);
        
            if (customer is null)
                return Result.Success();
        
            var stripeSubscription = await subscriptionService.GetAsync(session.SubscriptionId, cancellationToken: cancellationToken);
        
            var subscriptionItem = stripeSubscription.Items.Data.FirstOrDefault();
        
            if (subscriptionItem is null)
                return Result.Success();
            
            var stripePrice = await priceService.GetAsync(subscriptionItem.Price.Id, cancellationToken: cancellationToken);
            var stripeProduct = await productService.GetAsync(stripePrice.ProductId, cancellationToken: cancellationToken);

            var planName = stripeProduct.Name;
        
            var priceAmount = stripePrice.UnitAmount;
            var currency = stripePrice.Currency;
            var priceFormatted = $"${priceAmount / 100.0:F2} {currency.ToUpper()}";
            
            var status = MapStripeStatusToSubscriptionStatus(stripeSubscription.Status);
            
            DateTimeOffset currentPeriodStart = dateTimeProvider.FromDateTime(subscriptionItem.CurrentPeriodStart);
            DateTimeOffset currentPeriodEnd = dateTimeProvider.FromDateTime(subscriptionItem.CurrentPeriodEnd);

            Result subscriptionResult = customer.StartSubscription
            (
                stripeSubscription.Id,
                subscriptionItem.Price.Id,
                currentPeriodStart,
                currentPeriodEnd,
                dateTimeProvider
            );
        
            if (subscriptionResult.IsFailure)
                return Result.Failure(subscriptionResult.Error);
        
            if (status == SubscriptionStatus.Active)
            {
                Result activationResult = customer.ActivateSubscription
                (
                    stripeSubscription.Id,
                    planName,
                    priceFormatted,
                    currentPeriodStart,
                    currentPeriodEnd,
                    dateTimeProvider
                );
        
                if (activationResult.IsFailure)
                    return Result.Failure(activationResult.Error);
            }
        
            await context.SaveChangesAsync(cancellationToken);
        
            return Result.Success();
    }

    private async Task<Result> HandleSubscriptionUpdate(Event stripeEvent, CancellationToken cancellationToken)
    {
        var stripeSubscription = stripeEvent.Data.Object as Subscription;
        
        if (stripeSubscription is null)
            return Result.Success();
        
        var customer = await context.Customers
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.StripeCustomerId == stripeSubscription.CustomerId, cancellationToken);
        
        if (customer is null)
            return Result.Success();
        
        var existingSubscription = customer.Subscriptions
            .FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscription.Id);
        
        if (existingSubscription is null)
            return Result.Success();
        
        var subscriptionItem = stripeSubscription.Items.Data.FirstOrDefault();
        
        if (subscriptionItem is null)
            return Result.Success();
        
        DateTimeOffset currentPeriodStart = dateTimeProvider.FromDateTime(subscriptionItem.CurrentPeriodStart);
        DateTimeOffset currentPeriodEnd = dateTimeProvider.FromDateTime(subscriptionItem.CurrentPeriodEnd);
        
        var status = MapStripeStatusToSubscriptionStatus(stripeSubscription.Status);

        if (stripeSubscription.CancelAtPeriodEnd && status == SubscriptionStatus.Active)
        {
            Result updateResult = customer.CancelSubscriptionAtPeriodEnd
            (
                stripeSubscription.Id,
                currentPeriodStart,
                currentPeriodEnd,
                dateTimeProvider
            );
        
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);
        }
        else
        {
            Result updateResult = customer.UpdateSubscription(
                stripeSubscription.Id,
                status,
                currentPeriodStart,
                currentPeriodEnd,
                stripeSubscription.CancelAtPeriodEnd,
                dateTimeProvider
            );
        
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);
        }
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }

    private async Task<Result> HandleSubscriptionDeleted(Event stripeEvent, CancellationToken cancellationToken)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
        
        if (stripeSubscription is null)
            return Result.Success();
        
        var customer = await context.Customers
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.StripeCustomerId == stripeSubscription.CustomerId, cancellationToken);
        
        if (customer is null)
            return Result.Success();
        
        var existingSubscription = customer.Subscriptions
            .FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscription.Id);    
        
        if (existingSubscription is null)
            return Result.Success();
        
        var subscriptionItem = stripeSubscription.Items.Data.FirstOrDefault();

        if (subscriptionItem is null)
            return Result.Success();
        
        DateTimeOffset currentPeriodStart = dateTimeProvider.FromDateTime(subscriptionItem.CurrentPeriodStart);
        DateTimeOffset currentPeriodEnd = dateTimeProvider.FromDateTime(subscriptionItem.CurrentPeriodEnd);
        
        Result cancelResult = customer.CompleteSubscriptionCancellation(stripeSubscription.Id, currentPeriodStart, currentPeriodEnd, dateTimeProvider);
        
        if (cancelResult.IsFailure)
            return Result.Failure(cancelResult.Error);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
    
    private static SubscriptionStatus MapStripeStatusToSubscriptionStatus(string stripeStatus) => stripeStatus switch
    {
        "incomplete" => SubscriptionStatus.Incomplete,
        "active" => SubscriptionStatus.Active,
        "trialing" => SubscriptionStatus.Trialing,
        "canceled" => SubscriptionStatus.Cancelled,
        "unpaid" => SubscriptionStatus.Unpaid,
        "past_due" => SubscriptionStatus.PastDue,
        _ => SubscriptionStatus.Incomplete
    };
}
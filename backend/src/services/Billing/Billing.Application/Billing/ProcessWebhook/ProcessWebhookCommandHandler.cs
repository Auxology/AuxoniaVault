using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Abstractions.Services;
using Billing.Domain.Aggregate.Customer;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Billing.Application.Billing.ProcessWebhook;

internal sealed class ProcessWebhookCommandHandler(IStripeWebhookService webhookService, IBillingDbContext context, IDateTimeProvider dateTimeProvider) : ICommandHandler<ProcessWebhookCommand>
{
    public async Task<Result> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        Result<Event> eventResult = await webhookService.ConstructEventAsync(request.EventJson, request.Signature, cancellationToken);
        
        if (eventResult.IsFailure)
            return Result.Failure(eventResult.Error);
        
        var stripeEvent = eventResult.Value;
        
        return stripeEvent.Type switch
        {
            "checkout.session.completed" => await HandleSessionCompletion(stripeEvent, cancellationToken),
            _ => Result.Success()
        };
    }

    private async Task<Result> HandleSessionCompletion(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            var session = stripeEvent.Data.Object as Session;
        
            if (session is null)
                return Result.Success();
        
            var customer = await context.Customers
                .FirstOrDefaultAsync(c => c.StripeCustomerId == session.CustomerId, cancellationToken);
        
            if (customer is null)
                return Result.Success();
        
            var subscriptionService = new SubscriptionService();
            var stripeSubscription = await subscriptionService.GetAsync(session.SubscriptionId, cancellationToken: cancellationToken);
        
            var subscriptionItem = stripeSubscription.Items.Data.FirstOrDefault();
        
            if (subscriptionItem is null)
                return Result.Success();
        
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
        catch (Exception ex)
        {
            throw new Exception("Error processing checkout.session.completed webhook", ex);
        }
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
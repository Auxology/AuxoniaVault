using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Errors;
using Billing.Domain.Aggregate.Customer;
using Billing.Domain.Entities;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Webhooks.ProcessCheckoutSessionCompleted;

internal sealed class ProcessCheckoutSessionCompletedCommandHandler(IBillingDbContext context, IDateTimeProvider dateTimeProvider) : ICommandHandler<ProcessCheckoutSessionCompletedCommand>
{
    public async Task<Result> Handle(ProcessCheckoutSessionCompletedCommand request, CancellationToken cancellationToken)
    {
        Customer? customer = await context.Customers
            .Include(c => c.Subscriptions)
            .Include(c => c.SubscriptionHistories)
            .FirstOrDefaultAsync(c => c.StripeCustomerId == request.StripeCustomerId, cancellationToken);
        
        if (customer is null)
            return Result.Failure(CustomerErrors.CustomerNotFound);

        Result<Subscription> subscriptionResult = customer.StartSubscription
        (
            request.StripeSubscriptionId,
            request.StripeCustomerId,
            request.CurrentPeriodStart,
            request.CurrentPeriodEnd,
            dateTimeProvider
        );
        
        if (subscriptionResult.IsFailure)
            return Result.Failure(subscriptionResult.Error);

        Result activationResult = customer.ActivateSubscription
        (
            request.StripeSubscriptionId,
            request.ProductName,
            request.PriceFormatted,
            request.EventType,
            request.CurrentPeriodStart,
            request.CurrentPeriodEnd,
            dateTimeProvider
        );
        
        await context.SaveChangesAsync(cancellationToken);
        
        return activationResult;
    }
}
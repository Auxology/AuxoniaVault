using Billing.Application.Abstractions.Authentication;
using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Abstractions.Services;
using Billing.Application.Errors;
using Billing.Domain.Aggregate.Customer;
using Billing.Domain.Entities;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Billing.CancelSubscription;

internal sealed class CancelSubscriptionCommandHandler(
    IBillingDbContext context,
    IUserContext userContext,
    IStripeCheckoutService stripeCheckoutService,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<CancelSubscriptionCommand>
{
    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);

        Customer? customer = await context.Customers
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer is null)
            return Result.Failure(CustomerErrors.CustomerNotFound);

        Subscription? subscription = customer.Subscriptions
            .FirstOrDefault(s => s.StripeSubscriptionId == request.StripeSubscriptionId);
        
        if (subscription is null)
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);
        
        Result stripeResult = await stripeCheckoutService
            .CancelSubscriptionAtPeriodEndAsync(request.StripeSubscriptionId, cancellationToken);
        
        if (stripeResult.IsFailure)
            return Result.Failure(stripeResult.Error);

        Result domainResult = customer.CancelSubscriptionAtPeriodEnd(subscription.StripeSubscriptionId,
            subscription.CurrentPeriodEnd, subscription.CurrentPeriodStart, dateTimeProvider);
        
        if (domainResult.IsFailure)
            return Result.Failure(domainResult.Error);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
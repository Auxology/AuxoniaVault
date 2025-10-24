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

namespace Billing.Application.Billing.ResumeSubscription;

internal sealed class ResumeSubscriptionCommandHandler(
    IBillingDbContext context,
    IUserContext userContext,
    IStripeCheckoutService stripeCheckoutService,
    IDateTimeProvider dateTimeProvider
)
    : ICommandHandler<ResumeSubscriptionCommand>
{
    public async Task<Result> Handle(ResumeSubscriptionCommand request, CancellationToken cancellationToken)
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
        
        if (subscription.Status != SubscriptionStatus.Active)
            return Result.Failure(SubscriptionErrors.SubscriptionNotActive);
        
        if (!subscription.CancelAtPeriodEnd)
            return Result.Failure(SubscriptionErrors.SubscriptionNotScheduledForCancellation);

        var stripeResult =
            await stripeCheckoutService.ResumeSubscriptionAsync(request.StripeSubscriptionId, cancellationToken);
        
        if (stripeResult.IsFailure)
            return Result.Failure(stripeResult.Error);

        Result updateResult = customer.ResumeSubscription
        (
            request.StripeSubscriptionId,
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            dateTimeProvider
        );
        
        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
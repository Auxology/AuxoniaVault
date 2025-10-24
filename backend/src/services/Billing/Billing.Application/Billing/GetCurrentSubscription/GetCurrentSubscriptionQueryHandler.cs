using Billing.Application.Abstractions.Authentication;
using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Errors;
using Billing.Domain.Aggregate.Customer;
using Billing.Domain.Entities;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Billing.GetCurrentSubscription;

internal sealed class GetCurrentSubscriptionQueryHandler(IBillingDbContext context, IUserContext userContext) : IQueryHandler<GetCurrentSubscriptionQuery, CurrentSubscriptionReadModel>
{
    public async Task<Result<CurrentSubscriptionReadModel>> Handle(GetCurrentSubscriptionQuery request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);

        Customer? customer = await context.Customers
            .Include(c  => c.Subscriptions)
            .Include(c => c.SubscriptionHistories)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        
        if (customer is null)
            return Result.Failure<CurrentSubscriptionReadModel>(CustomerErrors.CustomerNotFound);

        Subscription? activeSubscription = customer.Subscriptions
            .FirstOrDefault(s => s.Status == SubscriptionStatus.Active);
        
        if (activeSubscription is null)
            return Result.Failure<CurrentSubscriptionReadModel>(SubscriptionErrors.NoActiveSubscription);

        SubscriptionHistory? currentSubscriptionHistory = customer.SubscriptionHistories
            .FirstOrDefault(sh => sh.StripeSubscriptionId == activeSubscription.StripeSubscriptionId);
        
        if (currentSubscriptionHistory is null)
            return Result.Failure<CurrentSubscriptionReadModel>(SubscriptionHistoryErrors.SubscriptionHistoryNotFound);

        var currentSubscription = new CurrentSubscriptionReadModel
        (
            activeSubscription.StripeSubscriptionId,
            activeSubscription.Status.ToString(),
            currentSubscriptionHistory.ProductName,
            currentSubscriptionHistory.PriceFormatted,
            activeSubscription.StripePriceId,
            activeSubscription.CurrentPeriodStart,
            activeSubscription.CurrentPeriodEnd,
            activeSubscription.CancelAtPeriodEnd,
            activeSubscription.CreatedAt,
            activeSubscription.UpdatedAt
        );
        
        return Result.Success(currentSubscription);
    }
}
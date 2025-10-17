using Billing.Domain.Aggregate.Customer;
using Billing.Domain.Errors;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;

namespace Billing.Domain.Entities;

public class Subscription : Entity
{
    public int Id { get; private set; }
    
    public string StripeCustomerId { get; private set; }
    
    public UserId UserId { get; private set; }
    
    public string StripeSubscriptionId { get; private set; }
    
    public string StripePriceId { get; private set; }
    
    public SubscriptionStatus Status { get; private set; }
    
    public DateTimeOffset CurrentPeriodStart { get; private set; }
    
    public DateTimeOffset CurrentPeriodEnd { get; private set; }
    
    public bool CancelAtPeriodEnd { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    private Subscription() { } // For EF Core

    private Subscription
    (
        UserId userId,
        string stripeCustomerId,
        string stripeSubscriptionId,
        string stripePriceId,
        SubscriptionStatus status,
        DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd,
        DateTimeOffset utcNow
    )
    
    {
        UserId = userId;
        StripeCustomerId = stripeCustomerId;
        StripeSubscriptionId = stripeSubscriptionId;
        StripePriceId = stripePriceId;
        Status = status;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        CancelAtPeriodEnd = false;
        CreatedAt = utcNow;
    }


    internal static Result<Subscription> CreateIncomplete(UserId userId, string stripeCustomerId, string stripeSubscriptionId,
        string stripePriceId, DateTimeOffset currentPeriodStart, DateTimeOffset currentPeriodEnd,
        IDateTimeProvider dateTimeProvider)
    {
        if (userId.IsEmpty())
            return Result.Failure<Subscription>(CustomerErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            return Result.Failure<Subscription>(CustomerErrors.StripeCustomerIdRequired);
        
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            return Result.Failure<Subscription>(SubscriptionErrors.StripeSubscriptionIdRequired);
        
        if (string.IsNullOrWhiteSpace(stripePriceId))
            return Result.Failure<Subscription>(SubscriptionErrors.StripePriceIdRequired);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        var subscription = new Subscription(
            userId,
            stripeCustomerId,
            stripeSubscriptionId,
            stripePriceId,
            SubscriptionStatus.Incomplete,
            currentPeriodStart,
            currentPeriodEnd,
            utcNow
        );
        
        return Result.Success(subscription);
    }

    internal Result Activate(DateTimeOffset currentPeriodStart, DateTimeOffset currentPeriodEnd,
        IDateTimeProvider dateTimeProvider)
    {
        if (Status == SubscriptionStatus.Active)
            return Result.Failure(SubscriptionErrors.SubscriptionAlreadyActive);
        
        Status = SubscriptionStatus.Active;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        CancelAtPeriodEnd = false;
        UpdatedAt = dateTimeProvider.UtcNow;
        
        return Result.Success();
    }

    internal Result Update
    (
        SubscriptionStatus newStatus,
        DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd,
        bool cancelAtPeriodEnd,
        IDateTimeProvider dateTimeProvider
    )

    {
        Status = newStatus;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        CancelAtPeriodEnd = cancelAtPeriodEnd;
        UpdatedAt = dateTimeProvider.UtcNow;

        return Result.Success();
    }

    internal Result Cancel(DateTimeOffset currentPeriodStart, DateTimeOffset currentPeriodEnd, IDateTimeProvider dateTimeProvider)
    {
        if (Status != SubscriptionStatus.Active)
            return Result.Failure(SubscriptionErrors.CannotCancelInactiveSubscription);
        
        CancelAtPeriodEnd = true;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        UpdatedAt = dateTimeProvider.UtcNow;
        
        return Result.Success();
    }
    
    internal Result MarkPastDue(IDateTimeProvider dateTimeProvider)
    {
        Status = SubscriptionStatus.PastDue;
        UpdatedAt = dateTimeProvider.UtcNow;
        
        return Result.Success();
    }
    
    internal Result Expire(IDateTimeProvider dateTimeProvider)
    {
        Status = SubscriptionStatus.Unpaid;
        UpdatedAt = dateTimeProvider.UtcNow;
        
        return Result.Success();
    }
}
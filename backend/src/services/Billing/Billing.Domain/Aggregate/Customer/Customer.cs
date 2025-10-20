using Billing.Domain.Entities;
using Billing.Domain.Errors;
using Billing.Domain.Events;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;

namespace Billing.Domain.Aggregate.Customer;

public class Customer : Entity, IAggregateRoot
{
    public int Id { get; private set; }
    
    public UserId UserId { get; private set; }
    
    public string StripeCustomerId { get; private set; }
    
    public string StripeCustomerName { get; private set; }
    
    public string StripeCustomerEmail { get; private set; }
    
    public ICollection<Subscription> Subscriptions { get; private set; }
    
    public ICollection<SubscriptionHistory> SubscriptionHistories { get; private set; }
    
    private Customer() { } // For EF Core
    
    private Customer(UserId userId, string stripeCustomerId, string stripeCustomerName, string stripeCustomerEmail)
    {
        UserId = userId;
        StripeCustomerId = stripeCustomerId;
        StripeCustomerName = stripeCustomerName;
        StripeCustomerEmail = stripeCustomerEmail;
        Subscriptions = [];
        SubscriptionHistories = [];
    }
    
    public static Result<Customer> Create(UserId userId, string stripeCustomerId, string stripeCustomerName, string stripeCustomerEmail)
    {
        if (userId.IsEmpty())
            return Result.Failure<Customer>(CustomerErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            return Result.Failure<Customer>(CustomerErrors.StripeCustomerIdRequired);
        
        if (string.IsNullOrWhiteSpace(stripeCustomerName))
            return Result.Failure<Customer>( CustomerErrors.StripeCustomerNameRequired);
        
        if (string.IsNullOrWhiteSpace(stripeCustomerEmail))
            return Result.Failure<Customer>( CustomerErrors.StripeCustomerEmailRequired);
        
        var customer = new Customer(userId, stripeCustomerId, stripeCustomerName, stripeCustomerEmail);
        
        return Result.Success(customer);
    }
    
    public Result<Subscription> StartSubscription(string stripeSubscriptionId, string stripePriceId, DateTimeOffset currentPeriodStart, DateTimeOffset currentPeriodEnd, IDateTimeProvider dateTimeProvider)
    {
        if (!Subscriptions.Any())
            Subscriptions = [];
        
        Subscription? existingSubscription = Subscriptions
            .FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscriptionId);
        
        if (existingSubscription is not null)
            return Result.Failure<Subscription>(SubscriptionErrors.SubscriptionAlreadyExists);
        
        Result<Subscription> subscriptionResult = Subscription.CreateIncomplete(
            UserId,
            StripeCustomerId,
            stripeSubscriptionId,
            stripePriceId,
            currentPeriodStart,
            currentPeriodEnd,
            dateTimeProvider
        );    
        
        if (subscriptionResult.IsFailure)
            return Result.Failure<Subscription>(subscriptionResult.Error);
        
        Subscriptions.Add(subscriptionResult.Value);
        
        return Result.Success(subscriptionResult.Value);
    }

    public Result ActivateSubscription(string stripeSubscriptionId, string productName, string priceFormatted, string eventType, DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd, IDateTimeProvider dateTimeProvider)
    {
        if (!Subscriptions.Any())
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);
        
        Subscription? subscription = Subscriptions
            .FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscriptionId);

        if (subscription is null)
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);

        Subscription? activeSubscription = Subscriptions
            .FirstOrDefault(s => s.Status == SubscriptionStatus.Active && s.Id != subscription.Id);
        
        if (activeSubscription is not null)
            return Result.Failure(SubscriptionErrors.ActiveSubscriptionAlreadyExists);
        
        Result activationResult = subscription.Activate(currentPeriodStart, currentPeriodEnd, dateTimeProvider);
        
        if (activationResult.IsFailure)
            return Result.Failure(activationResult.Error);

        Result<SubscriptionHistory> historyResult = SubscriptionHistory.Create
        (
            stripeSubscriptionId,
            productName,
            priceFormatted,
            eventType,
            currentPeriodStart,
            currentPeriodEnd
        );
        
        if (historyResult.IsFailure)
            return Result.Failure(historyResult.Error);
        
        SubscriptionHistories.Add(historyResult.Value);
        
        Raise(new SubscriptionActivatedDomainEvent
        (
            UserId.Value,
            StripeCustomerName,
            StripeCustomerEmail,
            stripeSubscriptionId,
            productName,
            priceFormatted,
            currentPeriodStart,
            currentPeriodEnd
        ));
        
        return Result.Success();
    }
    
    public Result UpdateSubscription(string stripeSubscriptionId, SubscriptionStatus newStatus,
        DateTimeOffset currentPeriodStart, DateTimeOffset currentPeriodEnd, bool cancelAtPeriodEnd,
        IDateTimeProvider dateTimeProvider)

    {
        if (!Subscriptions.Any())
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);
        
        Subscription? subscription = Subscriptions
            .FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscriptionId);

        if (subscription is null)
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);

        if (newStatus == subscription.Status)
            return Result.Failure(SubscriptionErrors.CannotUpdateToSameStatus);

        Result updateResult = subscription.Update(newStatus, currentPeriodStart, currentPeriodEnd, cancelAtPeriodEnd,
            dateTimeProvider);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        return Result.Success();
    }

    public Result CancelSubscriptionAtPeriodEnd
    (
        string stripeSubscriptionId,
        DateTimeOffset currentPeriodEnd,
        DateTimeOffset currentPeriodStart,
        IDateTimeProvider dateTimeProvider
    )

    {
        if (!Subscriptions.Any())
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);

        Subscription? subscription = Subscriptions
            .FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscriptionId);

        if (subscription is null)
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);

        if (subscription.Status != SubscriptionStatus.Active)
            return Result.Failure(SubscriptionErrors.OnlyActiveSubscriptionsCanBeCancelled);
        
        if (subscription.CancelAtPeriodEnd)
            return Result.Failure(SubscriptionErrors.SubscriptionAlreadyPendingCancellation);

        Result cancelResult = subscription.Update
        (
            SubscriptionStatus.Active,
            currentPeriodStart,
            currentPeriodEnd,
            cancelAtPeriodEnd: true, 
            dateTimeProvider
        );
        
        if (cancelResult.IsFailure)
            return cancelResult;
        
        return Result.Success();
    }

    public Result CompleteSubscriptionCancellation
    (
        string stripeSubscriptionId,
        string productName,
        string priceFormatted,
        DateTimeOffset currentPeriodEnd,
        DateTimeOffset currentPeriodStart,
        IDateTimeProvider dateTimeProvider
    )

    {
        if (!Subscriptions.Any())
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);

        Subscription? subscription = Subscriptions
            .FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscriptionId);

        if (subscription is null)
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);

        if (subscription.Status != SubscriptionStatus.Active)
            return Result.Failure(SubscriptionErrors.OnlyActiveSubscriptionsCanBeCancelled);

        Result cancelResult = subscription.Cancel(currentPeriodStart, currentPeriodEnd, dateTimeProvider);

        if (cancelResult.IsFailure)
            return cancelResult;
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        Raise(new SubscriptionCanceledDomainEvent
        (
            UserId.Value,
            StripeCustomerName,
            StripeCustomerEmail,
            stripeSubscriptionId,
            productName,
            priceFormatted,
            currentPeriodStart,
            currentPeriodEnd,
            utcNow
        ));

        return Result.Success();
    }
}
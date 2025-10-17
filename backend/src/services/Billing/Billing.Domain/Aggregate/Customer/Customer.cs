using Billing.Domain.Entities;
using Billing.Domain.Errors;
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
    
    private Customer() { } // For EF Core
    
    private Customer(UserId userId, string stripeCustomerId, string stripeCustomerName, string stripeCustomerEmail)
    {
        UserId = userId;
        StripeCustomerId = stripeCustomerId;
        StripeCustomerName = stripeCustomerName;
        StripeCustomerEmail = stripeCustomerEmail;
        Subscriptions = [];
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
        if  (string.IsNullOrWhiteSpace((stripeSubscriptionId)))
            return Result.Failure<Subscription>(SubscriptionErrors.StripeSubscriptionIdRequired);
        
        if (string.IsNullOrWhiteSpace((stripePriceId)))
            return Result.Failure<Subscription>(SubscriptionErrors.StripePriceIdRequired);
        
        if (Subscriptions is null)
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

    public Result ActivateSubscription(string stripeSubscriptionId, DateTimeOffset currentPeriodStart,
        DateTimeOffset currentPeriodEnd, IDateTimeProvider dateTimeProvider)
    {
        if (Subscriptions is null)
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
        
        // TODO: RAISE DOMAIN EVENT
        
        return Result.Success();
    }

    public Result UpdateSubscription(string stripeSubscriptionId, SubscriptionStatus newStatus,
        DateTimeOffset currentPeriodStart, DateTimeOffset currentPeriodEnd, bool cancelAtPeriodEnd,
        IDateTimeProvider dateTimeProvider)

    {
        if (Subscriptions is null)
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
    
    public Result CancelSubscription(
        string stripeSubscriptionId,
        IDateTimeProvider dateTimeProvider)
    {
        if (Subscriptions is null)
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);
        
        Subscription? subscription = Subscriptions
            .FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscriptionId);
    
        if (subscription is null)
            return Result.Failure(SubscriptionErrors.SubscriptionNotFound);
        
        if (subscription.Status != SubscriptionStatus.Active)
            return Result.Failure(SubscriptionErrors.OnlyActiveSubscriptionsCanBeCancelled);
    
        Result cancelResult = subscription.Cancel(dateTimeProvider);
    
        if (cancelResult.IsFailure)
            return cancelResult;
        
        // TODO: Raise Domain Event
        
        return Result.Success();
    }
}
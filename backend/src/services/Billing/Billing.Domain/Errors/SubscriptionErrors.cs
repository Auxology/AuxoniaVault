using Billing.SharedKernel;

namespace Billing.Domain.Errors;

internal static class SubscriptionErrors
{
    public static Error StripeSubscriptionIdRequired => Error.Validation
    (
        "Subscriptions.StripeSubscriptionIdRequired",
        "Stripe subscription ID is required."
    );
    
    public static Error StripePriceIdRequired => Error.Validation
    (
        "Subscriptions.StripePriceIdRequired",
        "Stripe price ID is required."
    );
    
    public static Error SubscriptionAlreadyActive => Error.Conflict
    (
        "Subscriptions.SubscriptionAlreadyActive",
        "There is already an active subscription."
    );
    
    public static Error SubscriptionAlreadyExists => Error.Conflict
    (
        "Subscriptions.SubscriptionAlreadyExists",
        "A subscription with this ID already exists."
    );

    public static Error SubscriptionNotFound => Error.NotFound
    (
        "Subscriptions.SubscriptionNotFound",
        "The subscription was not found."
    );

    public static Error ActiveSubscriptionAlreadyExists => Error.Conflict
    (
        "Subscriptions.ActiveSubscriptionAlreadyExists",
        "Customer already has an active subscription."
    );
    
    public static Error CannotCancelInactiveSubscription => Error.Validation
    (
        "Subscriptions.CannotCancelInactiveSubscription",
        "Only active subscriptions can be cancelled."
    );
    
    public static Error CannotUpdateToSameStatus => Error.Validation
    (
        "Subscriptions.CannotUpdateToSameStatus",
        "Cannot update subscription to the same status."
    );
    
    public static Error OnlyActiveSubscriptionsCanBeCancelled => Error.Validation
    (
        "Subscriptions.OnlyActiveSubscriptionsCanBeCancelled",
        "Only active subscriptions can be cancelled."
    );
    
    public static Error SubscriptionAlreadyPendingCancellation => Error.Conflict
    (
        "Subscriptions.SubscriptionAlreadyPendingCancellation",
        "The subscription is already pending cancellation at the end of the period."
    );
    
    public static Error SubscriptionNotActive => Error.Validation
    (
        "Subscriptions.SubscriptionNotActive",
        "Subscription is not active."
    );
    
    public static Error SubscriptionNotScheduledForCancellation => Error.Validation
    (
        "Subscriptions.SubscriptionNotScheduledForCancellation",
        "This subscription is not scheduled for cancellation."
    );
}
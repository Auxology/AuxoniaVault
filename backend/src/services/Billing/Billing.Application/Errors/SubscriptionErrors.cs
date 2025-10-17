using Billing.SharedKernel;

namespace Billing.Application.Errors;

internal static class SubscriptionErrors
{
    public static Error ActiveSubscriptionAlreadyExists => Error.Conflict
    (
        "Subscriptions.ActiveSubscriptionAlreadyExists",
        "Customer already has an active subscription."
    );
}
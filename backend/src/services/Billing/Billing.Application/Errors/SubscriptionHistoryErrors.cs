using Billing.SharedKernel;

namespace Billing.Application.Errors;

internal static class SubscriptionHistoryErrors
{
    public static Error SubscriptionHistoryNotFound => Error.NotFound
    (
        "SubscriptionHistories.SubscriptionHistoryNotFound",
        "Subscription history not found."
    );
}
using Billing.SharedKernel;

namespace Billing.Domain.Errors;

internal static class SubscriptionHistoryErrors
{
    public static Error StripeSubscriptionIdRequired => Error.Validation
    (
        "SubscriptionHistory.StripeSubscriptionIdRequired",
        "Stripe subscription ID is required for subscription history."
    );
    
    public static Error ProductNameRequired => Error.Validation
    (
        "SubscriptionHistory.ProductNameRequired",
        "Product name is required for subscription history."
    );
    
    public static Error PriceFormattedRequired => Error.Validation
    (
        "SubscriptionHistory.PriceFormattedRequired",
        "Price formatted is required for subscription history."
    );
}
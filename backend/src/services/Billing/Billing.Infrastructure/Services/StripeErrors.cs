using Billing.SharedKernel;

namespace Billing.Infrastructure.Services;

internal static class StripeErrors
{
    public static Error GeneralError(string message) => Error.Failure
    (
        "Stripe.GeneralFailure",
        $"General error: {message}"
    );
    
    public static Error StripeError(string code, string message) => Error.Failure
    (
        "Stripe.Failure",
        $"Stripe error ({code}): {message}"
    );
}
using Billing.SharedKernel;

namespace Billing.Application.Errors;

internal static class CustomerErrors
{
    public static Error CustomerNotInitialized => Error.NotFound
    (
        "Customer.NotInitialized",
        "The customer has not been initialized, please contact support if the issue persists."
    );
}
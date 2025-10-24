using Billing.SharedKernel;

namespace Billing.Domain.Errors;

internal static class CustomerErrors
{
    public static Error UserIdRequired => Error.Validation
    (
        "Customers.UserIdRequired",
        "The User Id is required."
    );
    
    public static Error StripeCustomerIdRequired => Error.Validation
    (
        "Customers.StripeCustomerIdRequired",
        "The Stripe Customer Id is required."
    );
    
    public static Error StripeCustomerNameRequired => Error.Validation
    (
        "Customers.StripeCustomerNameRequired",
        "The Stripe Customer Name is required."
    );
    
    public static Error StripeCustomerEmailRequired => Error.Validation
    (
        "Customers.StripeCustomerEmailRequired",
        "The Stripe Customer Email is required."
    );
}
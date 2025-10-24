namespace Billing.Domain.Aggregate.Customer;

public enum SubscriptionStatus
{
    Incomplete,
    Active,
    Trialing,
    Cancelled,
    Unpaid,
    PastDue
}
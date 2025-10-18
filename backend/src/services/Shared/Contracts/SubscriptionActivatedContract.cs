namespace Shared.Contracts;

public sealed record SubscriptionActivatedContract
(
    Guid UserId,
    string StripeCustomerName,
    string StripeCustomerEmail,
    string PriceFormatted,
    string PlanName,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd
);
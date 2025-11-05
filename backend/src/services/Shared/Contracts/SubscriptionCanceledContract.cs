namespace Shared.Contracts;

public sealed record SubscriptionCanceledContract
(
    Guid UserId,
    string StripeCustomerName,
    string StripeCustomerEmail,
    int Tier,
    string PriceFormatted,
    string PlanName,
    DateTimeOffset CanceledAt
);
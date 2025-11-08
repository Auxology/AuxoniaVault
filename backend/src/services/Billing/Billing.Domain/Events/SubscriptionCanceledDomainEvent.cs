using Billing.SharedKernel;

namespace Billing.Domain.Events;

public sealed record SubscriptionCanceledDomainEvent
(
    Guid UserId,
    string StripeCustomerName,
    string StripeCustomerEmail,
    string StripeSubscriptionId,
    string ProductName,
    string PriceFormatted,
    DateTimeOffset CanceledAt
) : IDomainEvent;
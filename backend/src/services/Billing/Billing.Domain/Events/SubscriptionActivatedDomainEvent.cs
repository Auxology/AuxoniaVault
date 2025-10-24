using Billing.SharedKernel;

namespace Billing.Domain.Events;

public sealed record SubscriptionActivatedDomainEvent
(
    Guid UserId,
    string StripeCustomerName,
    string StripeCustomerEmail,
    string StripeSubscriptionId,
    string ProductName,
    string PriceFormatted,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd
) : IDomainEvent;
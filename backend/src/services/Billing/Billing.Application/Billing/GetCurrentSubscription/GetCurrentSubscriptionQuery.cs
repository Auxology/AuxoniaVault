using Billing.Application.Abstractions.Messaging;

namespace Billing.Application.Billing.GetCurrentSubscription;

public sealed record GetCurrentSubscriptionQuery() : IQuery<CurrentSubscriptionReadModel>;

public sealed record CurrentSubscriptionReadModel
(
    string StripeSubscriptionId,
    string Status,
    string ProductName,
    string PriceFormatted,
    string StripePriceId,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd,
    bool CancelAtPeriodEnd,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
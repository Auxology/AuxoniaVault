using Billing.Application.Abstractions.Messaging;

namespace Billing.Application.Billing.GetUsersSubscriptionHistories;

public sealed record GetUsersSubscriptionHistoriesQuery() : IQuery<List<SubscriptionHistoryReadModel>>;

public sealed record SubscriptionHistoryReadModel
(
    int Id,
    string ProductName,
    string PriceFormatted,
    string EventType,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd
);
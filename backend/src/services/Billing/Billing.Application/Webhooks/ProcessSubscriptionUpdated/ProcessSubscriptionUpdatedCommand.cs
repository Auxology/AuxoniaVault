using Billing.Application.Abstractions.Messaging;
using Billing.Domain.Aggregate.Customer;

namespace Billing.Application.Webhooks.ProcessSubscriptionUpdated;

public sealed record ProcessSubscriptionUpdatedCommand
(
    string StripeCustomerId,
    string StripeSubscriptionId,
    SubscriptionStatus Status,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd,
    bool CancelAtPeriodEnd
) : ICommand;
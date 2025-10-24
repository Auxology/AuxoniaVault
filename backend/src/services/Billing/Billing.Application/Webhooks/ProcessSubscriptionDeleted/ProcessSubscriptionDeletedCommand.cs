using Billing.Application.Abstractions.Messaging;

namespace Billing.Application.Webhooks.ProcessSubscriptionDeleted;

public sealed record ProcessSubscriptionDeletedCommand
(
    string StripeCustomerId,
    string StripeSubscriptionId,
    string ProductName,
    string PriceFormatted,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd
) : ICommand;
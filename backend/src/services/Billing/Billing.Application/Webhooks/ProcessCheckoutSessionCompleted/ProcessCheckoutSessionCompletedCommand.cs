using Billing.Application.Abstractions.Messaging;

namespace Billing.Application.Webhooks.ProcessCheckoutSessionCompleted;

public sealed record ProcessCheckoutSessionCompletedCommand
(
    string StripeCustomerId,
    string StripeSubscriptionId,
    string StripePriceId,
    string ProductName,
    string PriceFormatted,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd
) : ICommand;
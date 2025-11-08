using Billing.Application.Abstractions.Messaging;

namespace Billing.Application.Billing.CreateCheckout;

public sealed record CreateCheckoutCommand(
    string StripePriceId
) : ICommand<string>;

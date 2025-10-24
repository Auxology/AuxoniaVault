using Billing.Application.Abstractions.Messaging;

namespace Billing.Application.Billing.CancelSubscription;

public sealed record CancelSubscriptionCommand(string StripeSubscriptionId) : ICommand; 
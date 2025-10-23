using Billing.Application.Abstractions.Messaging;

namespace Billing.Application.Billing.ResumeSubscription;

public sealed record ResumeSubscriptionCommand(string StripeSubscriptionId) : ICommand;
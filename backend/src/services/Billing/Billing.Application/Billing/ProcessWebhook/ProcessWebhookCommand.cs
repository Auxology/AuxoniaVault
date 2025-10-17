using Billing.Application.Abstractions.Messaging;

namespace Billing.Application.Billing.ProcessWebhook;

public sealed record ProcessWebhookCommand(string EventJson, string Signature) : ICommand;
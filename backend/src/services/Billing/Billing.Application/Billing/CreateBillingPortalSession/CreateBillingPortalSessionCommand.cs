using Billing.Application.Abstractions.Messaging;

namespace Billing.Application.Billing.CreateBillingPortalSession;

public sealed record CreateBillingPortalSessionCommand() : ICommand<string>;
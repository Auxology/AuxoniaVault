namespace Billing.Domain.Aggregate.Webhook;

public enum WebhookEventStatus
{
    Pending = 0,
    Processed = 1,
    Failed = 2
}
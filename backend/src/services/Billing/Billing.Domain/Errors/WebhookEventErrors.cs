using Billing.SharedKernel;

namespace Billing.Domain.Errors;

internal static class WebhookEventErrors
{
    public static readonly Error StripeEventIdRequired = Error.Validation
    (
        "WebhookEvents.StripeEventIdRequired",
        "Stripe event ID is required."
    );

    public static readonly Error EventTypeRequired = Error.Validation
    (
        "WebhookEvents.EventTypeRequired",
        "Event type is required."
    );

    public static readonly Error DuplicateWebhook = Error.Conflict
    (
        "WebhookEvents.DuplicateWebhook",
        "This webhook event has already been received."
    );

    public static readonly Error WebhookEventNotFound = Error.NotFound
    (
        "WebhookEvents.NotFound",
        "Webhook event was not found."
    );

    public static readonly Error AlreadyProcessed = Error.Conflict
    (
        "WebhookEvents.AlreadyProcessed",
        "This webhook event has already been processed."
    );

    public static readonly Error CannotFailProcessedEvent = Error.Conflict
    (
        "WebhookEvents.CannotFailProcessedEvent",
        "Cannot mark a processed webhook event as failed."
    );

    public static readonly Error MaxRetriesExceeded = Error.Conflict
    (
        "WebhookEvents.MaxRetriesExceeded",
        "Maximum retry attempts exceeded for this webhook event."
    );

    public static readonly Error InvalidEventType = Error.Validation
    (
        "WebhookEvents.InvalidEventType",
        "The webhook event type is not supported."
    );
}
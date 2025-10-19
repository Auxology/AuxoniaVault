using Billing.SharedKernel;

namespace Billing.Infrastructure.Webhooks;

internal static class WebhookErrors
{
    public static readonly Error StripeEventIdRequired = Error.Validation
    (
        "WebhookEvent.StripeEventIdRequired",
        "Stripe event ID is required."
    );

    public static readonly Error EventTypeRequired = Error.Validation
    (
        "WebhookEvent.EventTypeRequired",
        "Event type is required."
    );
    
    public static readonly Error DuplicateWebhook = Error.Conflict
    (
        "WebhookEvent.DuplicateWebhook",
        "This webhook event has already been received."
    );

    public static readonly Error WebhookEventNotFound = Error.NotFound
    (
        "WebhookEvent.NotFound",
        "Webhook event was not found."
    );

    public static readonly Error AlreadyProcessed = Error.Conflict
    (
        "WebhookEvent.AlreadyProcessed",
        "This webhook event has already been processed."
    );

    public static readonly Error CannotFailProcessedEvent = Error.Conflict
    (
        "WebhookEvent.CannotFailProcessedEvent",
        "Cannot mark a processed webhook event as failed."
    );

    public static readonly Error MaxRetriesExceeded = Error.Conflict
    (
        "WebhookEvent.MaxRetriesExceeded",
        "Maximum retry attempts exceeded for this webhook event."
    );

    public static readonly Error InvalidEventType = Error.Validation
    (
        "WebhookEvent.InvalidEventType",
        "The webhook event type is not supported."
    );

    public static readonly Error InvalidSignature = Error.Validation
    (
        "WebhookEvent.InvalidSignature",
        "The webhook signature is invalid."
    );

    public static readonly Error InvalidEventData = Error.Validation
    (
        "WebhookEvent.InvalidEventData",
        "The webhook event data is invalid or missing required fields."
    );
    
    public static readonly Error StripeApiError = Error.Failure
    (
        "WebhookEvent.StripeApiError",
        "An error occurred while communicating with the Stripe API."
    );
    
    public static Error GeneralError => Error.Failure
    (
        "WebhookEvent.GeneralError",
        "An unexpected error occurred while processing the webhook event."
    );
}
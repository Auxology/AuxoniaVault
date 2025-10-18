using Billing.Domain.Constants;
using Billing.Domain.Errors;
using Billing.SharedKernel;

namespace Billing.Domain.Aggregate.Webhook;

public class WebhookEvent : Entity, IAggregateRoot
{
    public int Id { get; private set; }

    public string StripeEventId { get; private set; }

    public string StripeEventType { get; private set; }

    public WebhookEventStatus Status { get; private set; }

    public int RetryCount { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    private WebhookEvent() { } // For EF Core

    private WebhookEvent(string stripeEventId, string stripeEventType, DateTimeOffset utcNow)
    {
        StripeEventId = stripeEventId;
        StripeEventType = stripeEventType;
        Status = WebhookEventStatus.Pending;
        RetryCount = 0;
        CreatedAt = utcNow;
    }

    public static Result<WebhookEvent> Creat(string stripeEventId, string stripeEventType,
        IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(stripeEventId))
            return Result.Failure<WebhookEvent>(WebhookEventErrors.StripeEventIdRequired);
        
        if (string.IsNullOrWhiteSpace(stripeEventType))
            return Result.Failure<WebhookEvent>(WebhookEventErrors.EventTypeRequired);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        var webhookEvent = new WebhookEvent(stripeEventId, stripeEventType, utcNow);
        
        return Result.Success(webhookEvent);
    }

    public Result MarkAsProcessed(IDateTimeProvider dateTimeProvider)
    {
        if (Status == WebhookEventStatus.Processed)
            return Result.Failure(WebhookEventErrors.AlreadyProcessed);
        
        Status = WebhookEventStatus.Processed;
        ProcessedAt = dateTimeProvider.UtcNow;
        ErrorMessage = null;
        
        return Result.Success();
    }
    
    public Result MarkAsFailed(string errorMessage)
    {
        if (Status == WebhookEventStatus.Processed)
            return Result.Failure(WebhookEventErrors.AlreadyProcessed);
        
        RetryCount++;
        Status = WebhookEventStatus.Failed;
        ErrorMessage = errorMessage;
        
        return Result.Success();
    }
    
    public bool CanRetry() => RetryCount < WebhookEventConstant.MaxRetryAttempts && Status == WebhookEventStatus.Failed;
}
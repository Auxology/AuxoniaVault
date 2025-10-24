using Billing.Infrastructure.Webhooks.Constants;
using Billing.SharedKernel;

namespace Billing.Infrastructure.Webhooks.ViewModels;

public sealed record SubscriptionProductInfoViewModel
(
    string PriceId,
    string ProductName,
    string PriceFormatted,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd
)
{
    public static SubscriptionProductInfoViewModel Empty(IDateTimeProvider dateTimeProvider) => new
    (
        PriceId: string.Empty,
        ProductName: WebhookConstants.DefaultProductName,
        PriceFormatted: WebhookConstants.DefaultPrice,
        CurrentPeriodStart: dateTimeProvider.UtcNow,
        CurrentPeriodEnd: dateTimeProvider.UtcNow
    );
}
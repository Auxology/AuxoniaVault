using Billing.Domain.Aggregate.Customer;
using Billing.Infrastructure.Webhooks.Constants;
using Billing.Infrastructure.Webhooks.ViewModels;
using Billing.SharedKernel;
using Stripe;

namespace Billing.Infrastructure.Webhooks.Services;

internal sealed class StripeWebhookMapper(IDateTimeProvider dateTimeProvider) : IStripeWebhookMapper
{
    public SubscriptionProductInfoViewModel ExtractProductInfo(Subscription subscription)
    {
        SubscriptionItem? firstItem = subscription.Items?.Data?.FirstOrDefault();

        if (firstItem is null)
            return SubscriptionProductInfoViewModel.Empty(dateTimeProvider);
        
        Price price = firstItem.Price;
        
        string priceId = price.Id;

        string productName = price.Product switch
        {
            Product product => product.Name ?? WebhookConstants.DefaultProductName,
            null => WebhookConstants.DefaultProductName,
        };

        
        string priceFormatted = Helpers.PriceFormatter.Format(price.UnitAmount, price.Currency);

        DateTimeOffset currentPeriodStart = dateTimeProvider.FromDateTime(firstItem.CurrentPeriodStart);
        DateTimeOffset currentPeriodEnd = dateTimeProvider.FromDateTime(firstItem.CurrentPeriodEnd);
        
        return new SubscriptionProductInfoViewModel
        (
            priceId,
            productName,
            priceFormatted,
            currentPeriodStart,
            currentPeriodEnd 
        );
    }

    public SubscriptionStatus MapStripeSubscriptionStatus(string stripeStatus)
    {
        return stripeStatus.ToLower() switch
        {
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Cancelled,
            "unpaid" => SubscriptionStatus.Unpaid,
            "incomplete" => SubscriptionStatus.Incomplete,
            "trialing" => SubscriptionStatus.Trialing,
            _ => SubscriptionStatus.Cancelled
        };
    }
}
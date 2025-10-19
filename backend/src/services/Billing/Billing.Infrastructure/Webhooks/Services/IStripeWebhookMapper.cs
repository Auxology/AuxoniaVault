using Billing.Infrastructure.Webhooks.ViewModels;
using Stripe;

namespace Billing.Infrastructure.Webhooks.Services;

public interface IStripeWebhookMapper
{
    SubscriptionProductInfoViewModel ExtractProductInfo(Subscription subscription);
}
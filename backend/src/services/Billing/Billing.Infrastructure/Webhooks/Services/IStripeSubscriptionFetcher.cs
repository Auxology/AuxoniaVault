using Billing.SharedKernel;
using Stripe;

namespace Billing.Infrastructure.Webhooks.Services;

public interface IStripeSubscriptionFetcher
{
    Task<Result<Subscription>> FetchWithDetailsAsync(string subscriptionId, CancellationToken cancellationToken);
}
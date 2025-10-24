using Billing.SharedKernel;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Billing.Infrastructure.Webhooks.Services;

internal sealed class StripeSubscriptionFetcher(SubscriptionService subscriptionService, ILogger<StripeSubscriptionFetcher> logger) : IStripeSubscriptionFetcher
{
    public async Task<Result<Subscription>> FetchWithDetailsAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Fetching subscription {SubscriptionId} from Stripe", subscriptionId);

            SubscriptionGetOptions options = new()
            {
                Expand = new List<string>
                {
                    "items.data.price.product",
                    "customer",
                    "latest_invoice"
                }
            };
            
            Subscription subscription = await subscriptionService.GetAsync(subscriptionId, options, cancellationToken: cancellationToken);
            
            logger.LogDebug("Successfully fetched subscription {SubscriptionId} from Stripe", subscriptionId);
            
            return Result.Success(subscription);
        }
        catch (StripeException ex)
        {
            logger.LogError
            (
                ex,
                "Stripe API error while fetching subscription {SubscriptionId}: {Message}",
                subscriptionId,
                ex.Message
            );
            
            return Result.Failure<Subscription>(WebhookErrors.StripeApiError);
        }
        catch (Exception ex)
        {
            logger.LogError
            (
                ex,
                "Unexpected error while fetching subscription {SubscriptionId}",
                subscriptionId
            );
            
            return Result.Failure<Subscription>(WebhookErrors.StripeApiError);
        }    
    }
}
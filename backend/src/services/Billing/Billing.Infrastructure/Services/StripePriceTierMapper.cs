using Billing.Domain.Aggregate.Customer;
using Billing.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Billing.Infrastructure.Services;

internal sealed class StripePriceTierMapper(IOptions<StripeSettings> settings) : IStripePriceTierMapper
{
    private readonly string _proPriceId = settings.Value.ProPriceId;
    
    public int GetTierFromPriceId(string stripePriceId)
    {
        return stripePriceId == _proPriceId ? (int)SubscriptionTier.Pro : (int)SubscriptionTier.Free;
    }
}
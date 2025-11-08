using Billing.Domain.Aggregate.Customer;

namespace Billing.Infrastructure.Services;

public interface IStripePriceTierMapper
{
    int GetTierFromPriceId(string stripePriceId);
}
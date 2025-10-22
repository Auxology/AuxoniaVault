using Billing.SharedKernel;

namespace Billing.Application.Abstractions.Services;

public interface IStripeBillingPortalService
{
    Task<Result<string>> CreateBillingPortalSessionAsync(string stripeCustomerId, CancellationToken cancellationToken);
}
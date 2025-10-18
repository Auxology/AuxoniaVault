using Billing.SharedKernel;

namespace Billing.Application.Abstractions.Services;

public interface IStripeCheckoutService
{
    Task<Result<string>> CreateCheckoutSessionAsync(string customerId, string priceId, CancellationToken cancellationToken);
}

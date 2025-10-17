using Billing.SharedKernel;
using Stripe;

namespace Billing.Application.Abstractions.Services;

public interface IStripeWebhookService
{
    Task<Result<Event>> ConstructEventAsync(string json, string signature, CancellationToken cancellationToken);
}
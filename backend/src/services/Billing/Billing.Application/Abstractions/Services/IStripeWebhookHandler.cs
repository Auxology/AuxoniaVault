using Billing.SharedKernel;

namespace Billing.Application.Abstractions.Services;

public interface IStripeWebhookHandler
{
    Task<Result> HandleAsync(string json, string signature, CancellationToken cancellationToken);
}
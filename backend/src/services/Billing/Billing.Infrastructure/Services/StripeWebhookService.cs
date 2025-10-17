using System.Text.Json;
using Billing.Application.Abstractions.Services;
using Billing.Infrastructure.Settings;
using Billing.SharedKernel;
using Microsoft.Extensions.Options;
using Stripe;

namespace Billing.Infrastructure.Services;

internal sealed class StripeWebhookService(IOptions<StripeSettings> options, IStripeClient stripeClient) : IStripeWebhookService
{
    public async Task<Result<Event>> ConstructEventAsync(string json, string signature, CancellationToken cancellationToken)
    {
        try
        {
            string webhookSecret = options.Value.WebhookSecret;
            
            if (string.IsNullOrEmpty(webhookSecret))
            {
                return Result.Failure<Event>(StripeErrors.GeneralError("Webhook secret is not configured"));
            }
            
            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret, throwOnApiVersionMismatch: false);
            
            return Result.Success(stripeEvent);
        }
        catch (StripeException ex)
        {
            if (ex.StripeError is not null)
            {
                return Result.Failure<Event>(StripeErrors.StripeVerificationError(ex.StripeError.Code, ex.StripeError.Message));
            }
            
            return Result.Failure<Event>(StripeErrors.GeneralError(ex.Message));
        }
        catch (JsonException)
        {
            return Result.Failure<Event>(StripeErrors.JsonError());
        }
        catch (Exception ex)
        {
            return Result.Failure<Event>(StripeErrors.GeneralError(ex.Message));
        }
    }
}
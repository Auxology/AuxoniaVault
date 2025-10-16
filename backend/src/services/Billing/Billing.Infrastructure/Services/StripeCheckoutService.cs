using Billing.Application.Abstractions.Services;
using Billing.Infrastructure.Settings;
using Billing.SharedKernel;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Billing.Infrastructure.Services;

internal sealed class StripeCheckoutService(IStripeClient stripeClient, IOptions<StripeSettings> stripeSettings) : IStripeCheckoutService
{
    public async Task<Result<string>> CreateCheckoutSessionAsync(string customerId, string priceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                Customer = customerId,
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SuccessUrl = stripeSettings.Value.SuccessUrl,
                CancelUrl = stripeSettings.Value.CancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "customer_id", customerId }
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "customer_id", customerId }
                    }
                }
            };
            
            var service = new SessionService(stripeClient);
            var session = await service.CreateAsync(options, null, cancellationToken);
            
            return Result.Success(session.Url);
        }
        catch (StripeException ex)
        {
            return Result.Failure<string>(StripeError(ex.StripeError.Code, ex.StripeError.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(GeneralError(ex.Message));
        }
    }

    internal static Error StripeError(string code, string message) => Error.Failure
    (
        "Stripe.Failure",
        $"Stripe error ({code}): {message}"
    );
    
    internal static Error GeneralError(string message) => Error.Failure
    (
        "Stripe.GeneralFailure",
        $"General error: {message}"
    );
}
using Billing.Application.Abstractions.Services;
using Billing.Infrastructure.Settings;
using Billing.SharedKernel;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Billing.Infrastructure.Services;

internal sealed class StripeCheckoutService(IOptions<StripeSettings> stripeSettings, SessionService sessionService ,SubscriptionService subscriptionService) : IStripeCheckoutService
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
             
            var session = await sessionService.CreateAsync(options, null, cancellationToken);
            
            return Result.Success(session.Url);
        }
        catch (StripeException ex)
        {
            return Result.Failure<string>(StripeErrors.StripeError(ex.StripeError.Code, ex.StripeError.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(StripeErrors.GeneralError(ex.Message));
        }
    }

    public async Task<Result> CancelSubscriptionAtPeriodEndAsync(string stripeSubscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            };
            
            await subscriptionService.UpdateAsync(stripeSubscriptionId, options, null, cancellationToken);
            
            return Result.Success();
        }        
        catch (StripeException ex)
        {
            return Result.Failure(StripeErrors.StripeError(ex.StripeError.Code, ex.StripeError.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure(StripeErrors.GeneralError(ex.Message));
        }
    }

    public async Task<Result> ResumeSubscriptionAsync(string stripeSubscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false
            };
            
            await subscriptionService.UpdateAsync(stripeSubscriptionId, options, null, cancellationToken);
            
            return Result.Success();
        }        
        catch (StripeException ex)
        {
            return Result.Failure(StripeErrors.StripeError(ex.StripeError.Code, ex.StripeError.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure(StripeErrors.GeneralError(ex.Message));
        }
    }
}
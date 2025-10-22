using Billing.Application.Abstractions.Services;
using Billing.Infrastructure.Settings;
using Billing.SharedKernel;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.BillingPortal;

namespace Billing.Infrastructure.Services;

internal sealed class StripeBillingPortalService(IOptions<StripeSettings> stripeSettings, SessionService sessionService) : IStripeBillingPortalService
{
    public async Task<Result<string>> CreateBillingPortalSessionAsync(string stripeCustomerId, CancellationToken cancellationToken)
    {
        try
        {
            string returnUrl = stripeSettings.Value.BillingPortalReturnUrl;
            
            SessionCreateOptions options = new SessionCreateOptions
            {
                Customer = stripeCustomerId,
                ReturnUrl = returnUrl 
            };
            
            Session session = await sessionService.CreateAsync(options, cancellationToken: cancellationToken);
            
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
}
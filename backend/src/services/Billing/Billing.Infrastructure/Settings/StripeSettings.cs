namespace Billing.Infrastructure.Settings;

public class StripeSettings
{
    public const string SectionName = "Stripe";
    
    public string SuccessUrl { get; init; } = string.Empty;
    
    public string CancelUrl { get; init; } = string.Empty;
    
    public string WebhookSecret { get; init; } = string.Empty;
    
    public string BillingPortalReturnUrl { get; init; } = string.Empty;
    
    public string ProPriceId { get; init; } = string.Empty;
}
namespace Billing.Infrastructure.Settings;

public class StripeSettings
{
    public const string SectionName = "Stripe";
    
    public string SuccessUrl { get; set; } = string.Empty;
    
    public string CancelUrl { get; set; } = string.Empty;
    
    public string WebhookSecret { get; set; } = string.Empty;
    
    public string BillingPortalReturnUrl { get; set; } = string.Empty;
}
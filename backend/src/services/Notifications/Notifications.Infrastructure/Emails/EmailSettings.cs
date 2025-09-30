namespace Notifications.Infrastructure.Emails;

public sealed class EmailSettings
{
    public const string ConfigurationSectionName = nameof(EmailSettings);
    
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "AuxoniaVault";
    public string CompanyName { get; set; } = "AuxoniaVault";
    public string SupportEmail { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public int TokenExpirationMinutes { get; set; } = 10;
}
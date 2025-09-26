namespace Notifications.Infrastructure.Emails;

public sealed class EmailSettings
{
    public const string ConfigurationSectionName = nameof(EmailSettings);
    
    public string SenderEmail { get; set; }
}
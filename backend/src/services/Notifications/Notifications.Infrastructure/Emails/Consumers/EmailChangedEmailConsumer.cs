using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts;

namespace Notifications.Infrastructure.Emails.Consumers;

public sealed class EmailChangedEmailConsumer(
    ILogger<EmailChangedEmailConsumer> logger,
    IAmazonSimpleEmailService simpleEmailService,
    IOptions<EmailSettings> emailSettings,
    EmailTemplateService emailTemplateService
) : IConsumer<EmailChangedContract>
{
    public async Task Consume(ConsumeContext<EmailChangedContract> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Sending email changed notification to {Email} for User {UserId} at {ChangedAt}", 
            message.NewEmail, message.UserId, message.ChangedAt); 
        
        try
        {
            var htmlContent = emailTemplateService.CreateEmailChangedNotificationTemplate(
                message.NewEmail,
                message.ChangedAt);
            
            var request = new SendEmailRequest
            {
                Source = $"{emailSettings.Value.SenderName} <{emailSettings.Value.SenderEmail}>",
                Destination = new Destination
                {
                    ToAddresses = [message.NewEmail]
                },
                Message = new Message
                {
                    Subject = new Content($"✅ {emailSettings.Value.CompanyName} Email Successfully Changed"),
                    Body = new Body
                    {
                        Html = new Content(htmlContent)
                    }
                }
            };
            
            await simpleEmailService.SendEmailAsync(request, context.CancellationToken);
            
            logger.LogInformation("Email changed notification sent successfully to {Email} for User {UserId}", 
                message.NewEmail, message.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email changed notification to {Email} for User {UserId}", 
                message.NewEmail, message.UserId);
            throw;
        }
    }
}

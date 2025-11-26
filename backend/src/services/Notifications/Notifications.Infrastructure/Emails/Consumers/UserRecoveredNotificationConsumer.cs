using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts;

namespace Notifications.Infrastructure.Emails.Consumers;

public sealed class UserRecoveredNotificationConsumer
(
    ILogger<UserRecoveredNotificationConsumer> logger,
    IAmazonSimpleEmailService simpleEmailService,
    IOptions<EmailSettings> emailSettings,
    EmailTemplateService emailTemplateService
) : IConsumer<UserRecoveredContract>
{
    public async Task Consume(ConsumeContext<UserRecoveredContract> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Sending user recovered email to {Email}", message.NewEmail);

        try
        {
            var htmlContent = emailTemplateService.CreateUserRecoveredTemplate
            (
                userId: message.UserId,
                newEmail: message.NewEmail,
                ipAddress: message.IpAddress,
                userAgent: message.UserAgent,
                recoveredAt: message.RecoveredAt
            );
            
            var request = new SendEmailRequest
            {
                Source = $"{emailSettings.Value.SenderName} <{emailSettings.Value.SenderEmail}>",
                Destination = new Destination
                {
                    ToAddresses = [message.NewEmail]
                },
                Message = new Message
                {
                    Subject = new Content($"🔓 Your {emailSettings.Value.CompanyName} Account Has Been Recovered"),
                    Body = new Body
                    {
                        Html = new Content(htmlContent)
                    }
                }
            };
            
            await simpleEmailService.SendEmailAsync(request, context.CancellationToken);
        }
        
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send user recovered email to {Email}",
                message.NewEmail);
        }
    }
}
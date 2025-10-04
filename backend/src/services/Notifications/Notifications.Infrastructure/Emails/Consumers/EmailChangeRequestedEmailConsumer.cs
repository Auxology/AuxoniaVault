using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts;

namespace Notifications.Infrastructure.Emails.Consumers;

public sealed class EmailChangeRequestedEmailConsumer(
    ILogger<EmailChangeRequestedEmailConsumer> logger,
    IAmazonSimpleEmailService simpleEmailService,
    IOptions<EmailSettings> emailSettings,
    EmailTemplateService emailTemplateService
) : IConsumer<EmailChangeRequestedContract>
{
    public async Task Consume(ConsumeContext<EmailChangeRequestedContract> context)
    {
        var message = context.Message;

        logger.LogInformation("Sending email change verification email to {Email} from IP {IpAddress} at {RequestedAt}",
            message.CurrentEmail, message.IpAddress, message.RequestedAt);

        try
        {
            var htmlContent = emailTemplateService.CreateEmailChangeVerificationTemplate(
                message.CurrentEmail,
                message.CurrentOtp,
                message.IpAddress,
                message.UserAgent,
                message.RequestedAt);

            var request = new SendEmailRequest
            {
                Source = $"{emailSettings.Value.SenderName} <{emailSettings.Value.SenderEmail}>",
                Destination = new Destination
                {
                    ToAddresses = [message.CurrentEmail]
                },
                Message = new Message
                {
                    Subject = new Content($"📧 {emailSettings.Value.CompanyName} Email Change Verification"),
                    Body = new Body
                    {
                        Html = new Content(htmlContent)
                    }
                }
            };

            await simpleEmailService.SendEmailAsync(request, context.CancellationToken);

            logger.LogInformation("Email change verification email sent successfully to {Email}",
                message.CurrentEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email change verification email to {Email}",
                message.CurrentEmail);
            throw;
        }
    }
}
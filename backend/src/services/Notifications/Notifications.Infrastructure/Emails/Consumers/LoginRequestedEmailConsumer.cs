using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts;

namespace Notifications.Infrastructure.Emails.Consumers;

public sealed class LoginRequestedEmailConsumer(
    ILogger<LoginRequestedEmailConsumer> logger,
    IAmazonSimpleEmailService simpleEmailService,
    IOptions<EmailSettings> emailSettings,
    EmailTemplateService emailTemplateService
) : IConsumer<LoginRequestedContract>
{
    public async Task Consume(ConsumeContext<LoginRequestedContract> context)
    {
        var message = context.Message;

        logger.LogInformation("Sending login email to {Email} requested at {RequestedAt}",
            message.Email, message.RequestedAt);

        try
        {
            var htmlContent = emailTemplateService.CreateLoginEmailTemplate(
                message.Email,
                message.Token,
                message.RequestedAt);

            var request = new SendEmailRequest
            {
                Source = $"{emailSettings.Value.SenderName} <{emailSettings.Value.SenderEmail}>",
                Destination = new Destination
                {
                    ToAddresses = [message.Email]
                },
                Message = new Message
                {
                    Subject = new Content($"🔐 Your {emailSettings.Value.CompanyName} Login Token"),
                    Body = new Body
                    {
                        Html = new Content(htmlContent)
                    }
                }
            };

            await simpleEmailService.SendEmailAsync(request, context.CancellationToken);

            logger.LogInformation("Login email sent successfully to {Email}",
                message.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send login email to {Email}",
                message.Email);
            throw;
        }
    }
}
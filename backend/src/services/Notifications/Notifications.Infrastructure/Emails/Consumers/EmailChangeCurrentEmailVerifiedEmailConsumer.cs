using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts;

namespace Notifications.Infrastructure.Emails.Consumers;

public sealed class EmailChangeCurrentEmailVerifiedEmailConsumer(
    ILogger<EmailChangeCurrentEmailVerifiedEmailConsumer> logger,
    IAmazonSimpleEmailService simpleEmailService,
    IOptions<EmailSettings> emailSettings,
    EmailTemplateService emailTemplateService
) : IConsumer<EmailChangeCurrentEmailVerifiedContract>
{
    public async Task Consume(ConsumeContext<EmailChangeCurrentEmailVerifiedContract> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Sending new email verification email to {Email} at {RequestedAt}", 
            message.newEmail, message.RequestedAt); 
        
        try
        {
            var htmlContent = emailTemplateService.CreateEmailChangeNewEmailVerificationTemplate(
                message.newEmail,
                message.newOtp,
                message.RequestedAt);
            
            var request = new SendEmailRequest
            {
                Source = $"{emailSettings.Value.SenderName} <{emailSettings.Value.SenderEmail}>",
                Destination = new Destination
                {
                    ToAddresses = [message.newEmail]
                },
                Message = new Message
                {
                    Subject = new Content($"✅ {emailSettings.Value.CompanyName} New Email Verification"),
                    Body = new Body
                    {
                        Html = new Content(htmlContent)
                    }
                }
            };
            
            await simpleEmailService.SendEmailAsync(request, context.CancellationToken);
            
            logger.LogInformation("New email verification email sent successfully to {Email}", 
                message.newEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send new email verification email to {Email}", 
                message.newEmail);
            throw;
        }
    }
}

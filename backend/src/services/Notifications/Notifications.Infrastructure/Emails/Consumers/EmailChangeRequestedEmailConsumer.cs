using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts;

namespace Notifications.Infrastructure.Emails.Consumers;

public sealed class EmailChangeRequestedEmailConsumer(
    ILogger<LoginRequestedEmailConsumer> logger,
    IAmazonSimpleEmailService simpleEmailService,
    IOptions<EmailSettings> emailSettings
) : IConsumer<EmailChangeRequestedContract>
{
    public async Task Consume(ConsumeContext<EmailChangeRequestedContract> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Sending email change verification email to {Email}", message.CurrentEmail); 
        
        var request = new SendEmailRequest
        {
            Source = emailSettings.Value.SenderEmail,
            Destination = new Destination
            {
                ToAddresses = [message.CurrentEmail]
            },
            Message = new Message
            {
                Subject = new Content("Your email change verification code"),
                Body = new Body
                {
                    Html = new Content
                    (
                        $"<p>Your email change verification code is: <strong>{message.CurrentOtp}</strong></p><p>This code will expire in 10 minutes.</p><p>If you did not request to change your email, please revoke all the sessions and improve security of your email.</p>"
                    )
                }
            }
        };
        
        await simpleEmailService.SendEmailAsync(request, context.CancellationToken);
        
        logger.LogInformation("Email change verification email sent to {Email}", message.CurrentEmail);
    }
}
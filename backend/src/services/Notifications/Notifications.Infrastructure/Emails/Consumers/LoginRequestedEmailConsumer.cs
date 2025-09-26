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
    IOptions<EmailSettings> emailSettings
) : IConsumer<LoginRequestedContract>
{
    public async Task Consume(ConsumeContext<LoginRequestedContract> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Sending login email to {Email}", message.Email);
        
        var request = new SendEmailRequest
        {
            Source = emailSettings.Value.SenderEmail,
            Destination = new Destination
            {
                ToAddresses = [message.Email]
            },
            Message = new Message
            {
                Subject = new Content("Your login token"),
                Body = new Body
                {
                    Html = new Content
                    (
                        $"<p>Your login token is: <strong>{message.Token}</strong></p><p>This token will expire in 10 minutes.</p>"
                    )
                }
            }
        };
        
        await simpleEmailService.SendEmailAsync(request, context.CancellationToken);
        
        logger.LogInformation("Login email sent to {Email}", message.Email);
    }
}
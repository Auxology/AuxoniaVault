using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts;

namespace Notifications.Infrastructure.Emails.Consumers;

public sealed class SubscriptionActivatedEmailConsumer
(
    ILogger<SubscriptionActivatedEmailConsumer> logger,
    IAmazonSimpleEmailService simpleEmailService,
    IOptions<EmailSettings> emailSettings,
    EmailTemplateService emailTemplateService
) : IConsumer<SubscriptionActivatedContract>
{
    public async Task Consume(ConsumeContext<SubscriptionActivatedContract> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Sending subscription activated email to {Email} for plan {PlanName}",
            message.StripeCustomerName, message.PlanName);

        try
        {
            var htmlContent = emailTemplateService.CreateSubscriptionActivatedTemplate
            (
                message.StripeCustomerName,
                message.StripeCustomerEmail,
                message.PlanName,
                message.PriceFormatted,
                message.CurrentPeriodStart,
                message.CurrentPeriodEnd
            );
            
            var request = new SendEmailRequest
            {
                Source = $"{emailSettings.Value.SenderName} <{emailSettings.Value.SenderEmail}>",
                Destination = new Destination
                {
                    ToAddresses = [message.StripeCustomerEmail]
                },
                Message = new Message
                {
                    Subject = new Content($"🎉 Your {emailSettings.Value.CompanyName} Subscription is Active!"),
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
            logger.LogError(ex, "Failed to send subscription activated email to {Email}",
                message.StripeCustomerEmail);
            throw;
        }
    }
}
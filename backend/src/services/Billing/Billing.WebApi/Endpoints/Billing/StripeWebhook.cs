using Billing.Application.Billing.ProcessWebhook;
using Billing.WebApi.Infrastructure;
using MediatR;

namespace Billing.WebApi.Endpoints.Billing;

internal sealed class StripeWebhook : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/billing/webhooks/stripe", async
        (
            HttpContext httpContext,
            ISender sender,
            ILogger<StripeWebhook> logger
        ) =>
        {            
            logger.LogInformation("Received Stripe webhook");
            
            using var reader = new StreamReader(httpContext.Request.Body);
            var eventJson = await reader.ReadToEndAsync();
            
            
            if (!httpContext.Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader))
            {
                return Results.BadRequest("Missing Stripe-Signature header.");
            }

            var signature = signatureHeader.ToString();
            
            var command = new ProcessWebhookCommand(eventJson, signature);
            
            var result = await sender.Send(command);
            
            return Results.Ok();
        })
        .AllowAnonymous()
        .WithTags(Tags.StripeWebhook);
    }
}
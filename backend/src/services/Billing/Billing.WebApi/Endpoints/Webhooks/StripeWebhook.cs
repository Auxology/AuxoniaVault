using Billing.Application.Abstractions.Services;
using Billing.SharedKernel;
using Billing.WebApi.Infrastructure;

namespace Billing.WebApi.Endpoints.Webhooks;

internal sealed class StripeWebhook : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/billing/webhooks/stripe", async
        (
            HttpContext httpContext,
            IStripeWebhookHandler handler,
            CancellationToken cancellationToken
        ) =>
        {
            string json;
            using (StreamReader reader = new(httpContext.Request.Body))
            {
                json = await reader.ReadToEndAsync(cancellationToken);
            }


            if (!httpContext.Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader))
                return Results.BadRequest("Missing Stripe-Signature header");

            string signature = signatureHeader.ToString();

            Result result = await handler.HandleAsync(json, signature, cancellationToken);

            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
        })
        .AllowAnonymous()
        .WithName("StripeWebhook")
        .WithTags(Tags.StripeWebhook)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Stripe webhook endpoint";
            operation.Description = "Receives and processes Stripe webhook events";
            return operation;
        });
    }
}
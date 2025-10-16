using Billing.Application.Billing.CreateCheckout;
using Billing.SharedKernel;
using Billing.WebApi.Infrastructure;
using MediatR;

namespace Billing.WebApi.Endpoints.Billing;

internal sealed class CreateCheckout : IEndpoint
{
    private sealed record Request(string PriceId);
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/billing/checkout-sessions", async
        (
            HttpContext httpContext,
            ISender sender,
            Request request
        ) =>
        {
            var command = new CreateCheckoutCommand(request.PriceId);

            Result<string> result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithTags(Tags.Billing);
    }
}
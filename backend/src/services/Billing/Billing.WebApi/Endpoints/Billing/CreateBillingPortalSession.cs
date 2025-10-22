using Billing.Application.Billing.CreateBillingPortalSession;
using Billing.SharedKernel;
using Billing.WebApi.Infrastructure;
using MediatR;

namespace Billing.WebApi.Endpoints.Billing;

internal sealed class CreatePortalSession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/billing/portal-sessions", async 
        (
            HttpContext httpContext, 
            ISender sender
        )
        =>
        {
            var command = new CreateBillingPortalSessionCommand();

            Result<string> result = await sender.Send(command);
            
            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithName("CreatePortalSession")
        .WithTags(Tags.Billing)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Create a Stripe billing portal session";
            operation.Description = "Creates a new Stripe billing portal session where users can manage their payment methods, subscriptions, and billing information. Returns a URL to redirect the user to.";
            return operation;
        });
    }
}
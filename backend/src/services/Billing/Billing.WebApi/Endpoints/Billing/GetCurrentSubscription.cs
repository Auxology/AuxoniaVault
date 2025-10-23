using Billing.Application.Billing.GetCurrentSubscription;
using Billing.SharedKernel;
using Billing.WebApi.Infrastructure;
using MediatR;

namespace Billing.WebApi.Endpoints.Billing;

internal sealed class CheckSubscriptionStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/billing/subscription/status", async
        (
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var command = new GetCurrentSubscriptionQuery();
            
            Result<CurrentSubscriptionReadModel> result = await sender.Send(command);
            
            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithName("CheckSubscriptionStatus")
        .WithTags(Tags.Billing)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Check if user has active subscription";
            operation.Description = "Quick check to determine if the authenticated user has an active subscription. Useful for feature gating.";
            return operation;
        });
    }
}
using Billing.Application.Billing.GetUsersSubscriptionHistories;
using Billing.SharedKernel;
using Billing.WebApi.Infrastructure;
using MediatR;

namespace Billing.WebApi.Endpoints.Billing;

internal sealed class GetUserSubscriptionHistories : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/billing/subscription-histories", async
        (
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var command = new GetUsersSubscriptionHistoriesQuery();
            
            Result<List<SubscriptionHistoryReadModel>> result = await sender.Send(command);
            
            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithName("GetUserSubscriptionHistories")
        .WithTags(Tags.Billing)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get user's subscription histories";
            operation.Description = "Retrieves the subscription histories for the authenticated user";
            return operation;
        });
    }
}
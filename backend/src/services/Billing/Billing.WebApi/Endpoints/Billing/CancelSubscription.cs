using Billing.Application.Billing.CancelSubscription;
using Billing.SharedKernel;
using Billing.WebApi.Infrastructure;
using MediatR;

namespace Billing.WebApi.Endpoints.Billing;

internal sealed class CancelSubscription : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/billing/subscriptions/{subscriptionId}/cancel", async
        (
            string subscriptionId,
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var command = new CancelSubscriptionCommand(subscriptionId);

            Result result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithName("CancelSubscription")
        .WithTags(Tags.Billing)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Cancel a subscription at period end";
            operation.Description = "Requests cancellation of a subscription. The user keeps access until the current billing period ends, then the subscription is cancelled.";
            return operation;
        });
    }
}
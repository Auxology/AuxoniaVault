using Billing.Application.Billing.ResumeSubscription;
using Billing.SharedKernel;
using Billing.WebApi.Infrastructure;
using MediatR;

namespace Billing.WebApi.Endpoints.Billing;

internal sealed class ResumeSubscription : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/billing/subscriptions/{subscriptionId}/resume", async
            (
                string subscriptionId,
                HttpContext httpContext,
                ISender sender
            ) =>
            {
                var command = new ResumeSubscriptionCommand(subscriptionId);

                Result result = await sender.Send(command);

                return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
            })
            .RequireAuthorization()
            .WithName("ResumeSubscription")
            .WithTags(Tags.Billing)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Resume a subscription";
                operation.Description = "Requests resume of a subscription. The user will have access to the subscription again.";
                return operation;
            });
    }
}
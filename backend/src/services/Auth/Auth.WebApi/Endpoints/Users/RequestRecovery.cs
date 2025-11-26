using Auth.Application.Users.RequestRecovery;
using Auth.SharedKernel;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class RequestRecovery : IEndpoint
{
    private sealed record Request(string RecoveryCode, Guid UserId);
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/recovery", async
        (
            Request request,
            ISender sender,
            HttpContext httpContext
        ) =>
        {
            var requestMetadata = httpContext.GetRequestMetadata();

            var command = new RequestRecoveryCommand(request.UserId, request.RecoveryCode, requestMetadata);

            Result<string> result = await sender.Send(command);

            return result.IsSuccess ? Results.Accepted(value: result.Value) : CustomResults.Problem(result, httpContext);
        })
        .WithTags(Tags.Users);
        }
}
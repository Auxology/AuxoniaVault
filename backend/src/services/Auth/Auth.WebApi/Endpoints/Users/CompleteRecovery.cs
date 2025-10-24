using Auth.Application.Users.CompleteRecovery;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class CompleteRecovery : IEndpoint
{
    private sealed record Request(string NewEmail, string UniqueIdentifier);


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/complete-recovery", async
        (
            Request request,
            ISender sender,
            HttpContext httpContext
        ) =>
        {
            var command = new CompleteRecoveryCommand(request.NewEmail, request.UniqueIdentifier);
            
            var result = await sender.Send(command);
            
            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
        })
        .WithTags(Tags.Users);
    }
}
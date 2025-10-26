using Auth.Application.Users.EmailChangeVerifyCurrent;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class EmailChangeVerifyCurrent : IEndpoint
{
    private sealed record Request(int CurrentOtp);
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/email/verify-current", async (
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken
        )
        =>
        {
            var requestMetadata = httpContext.GetRequestMetadata();
            
            var command = new EmailChangeVerifyCurrentCommand(request.CurrentOtp, requestMetadata);

            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
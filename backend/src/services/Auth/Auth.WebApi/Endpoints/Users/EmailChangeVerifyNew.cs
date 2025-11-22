using Auth.Application.Users.EmailChangeVerifyNew;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class EmailChangeVerifyNew : IEndpoint
{
    private sealed record Request(int NewOtp);


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/users/me/email/new/verification", async
            (
                Request request,
                ISender sender,
                HttpContext httpContext
            ) =>
            {
                var requestMetadata = httpContext.GetRequestMetadata();
                
                var command = new EmailChangeVerifyNewCommand(request.NewOtp, requestMetadata);

                var result = await sender.Send(command, httpContext.RequestAborted);

                return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
            })
            .RequireAuthorization()
            .WithTags(Tags.Users);
    }
}
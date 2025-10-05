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
        app.MapPost("api/auth/email/verify-new", async
            (
                Request request,
                ISender sender,
                HttpContext httpContext
            ) =>
            {
                var command = new EmailChangeVerifyNewCommand(request.NewOtp);

                var result = await sender.Send(command, httpContext.RequestAborted);

                return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
            })
            .RequireAuthorization()
            .WithTags(Tags.Users);
    }
}
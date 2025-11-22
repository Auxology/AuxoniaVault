using Auth.Application.Users.Logout;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/auth/sessions/current", async (
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var refreshToken = httpContext.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Results.NoContent();

            var command = new LogoutCommand(refreshToken);

            var result = await sender.Send(command);

            if (result.IsFailure)
                return CustomResults.Problem(result, httpContext);

            httpContext.RemoveAuthenticationCookie();

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
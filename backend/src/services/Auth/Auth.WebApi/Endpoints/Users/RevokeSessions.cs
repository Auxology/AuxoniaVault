using Auth.Application.Users.RevokeSessions;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class RevokeSessions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/auth/users/revoke-sessions", async
        (
            HttpContext httpContext,
            [FromServices] ISender sender
        ) =>
        {
            string? refreshToken = httpContext.GetRefreshTokenFromCookie();
            
            if (string.IsNullOrEmpty(refreshToken))
                return Results.Unauthorized();
            
            var command = new RevokeSessionsCommand(refreshToken);

            var result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
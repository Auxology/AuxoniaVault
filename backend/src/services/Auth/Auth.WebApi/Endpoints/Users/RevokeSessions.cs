using Auth.Application.Users.RevokeSessions;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class RevokeSessions : IEndpoint
{
    private sealed record Request(string RefreshToken);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/auth/users/revoke-sessions", async
        (
            [FromBody] Request request,
            HttpContext httpContext,
            [FromServices] ISender sender
        ) =>
        {
            var command = new RevokeSessionsCommand(request.RefreshToken);

            var result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
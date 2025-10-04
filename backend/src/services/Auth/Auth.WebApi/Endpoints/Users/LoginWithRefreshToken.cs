using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Users.LoginWithRefreshToken;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class LoginWithRefreshToken : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/refresh-token", async
        (
            HttpContext httpContext,
            ISender sender
        )
        =>
        {
            var requestMetadata = httpContext.GetRequestMetadata();
            
            string? refreshToken = httpContext.GetRefreshTokenFromCookie();
            
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                httpContext.RemoveAuthenticationCookie();
                return Results.Unauthorized();
            }

            var command = new LoginWithRefreshTokenCommand(refreshToken, requestMetadata);

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                httpContext.RemoveAuthenticationCookie();
                return CustomResults.Problem(result, httpContext);
            }

            httpContext.SetAuthenticationCookie(result.Value.RefreshToken);

            return Results.Ok(new { accessToken = result.Value.AccessToken });

        })
        .WithTags(Tags.Users);
    }
}
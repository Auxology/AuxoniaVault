using Auth.Application.Users.LoginWithRefreshToken;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;
using Shared.Requests;
using LoginWithRefreshTokenResponse = Shared.Responses.LoginWithRefreshTokenResponse;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class LoginWithRefreshToken : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh-token", async
        (
            HttpContext httpContext,
            ISender sender,
            LoginWithRefreshTokenRequest request
        )
        =>
        {
            var requestMetadata = httpContext.GetRequestMetadata();
            
            var command = new LoginWithRefreshTokenCommand(request.RefreshToken, requestMetadata);

            var result = await sender.Send(command);

            if (result.IsFailure)
                return CustomResults.Problem(result, httpContext);

            LoginWithRefreshTokenResponse response = new
            (
                AccessToken: result.Value.AccessToken,
                RefreshToken: result.Value.RefreshToken
            );
            
            return Results.Ok(response);

        })
        .WithTags(Tags.Users);
    }
}
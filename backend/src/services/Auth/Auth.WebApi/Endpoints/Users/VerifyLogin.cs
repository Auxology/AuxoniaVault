using Auth.Application.Users.VerifyLogin;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;
using Shared.Requests;
using VerifyLoginResponse = Shared.Requests.VerifyLoginResponse;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class VerifyLogin : IEndpoint
{
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/verify-login", async (
            VerifyLoginRequest request,
            HttpContext httpContext,
            ISender sender) =>
        {
            var requestMetadata = httpContext.GetRequestMetadata();

            var command = new VerifyLoginCommand(request.Email, request.Code, requestMetadata);

            var result = await sender.Send(command);

            if (result.IsFailure)
                return CustomResults.Problem(result, httpContext);

            VerifyLoginResponse response = new
            (
                AccessToken: result.Value.AccessToken,
                RefreshToken: result.Value.RefreshToken
            );

            return Results.Ok(response);
        })
        .WithTags(Tags.Users);
    }
}
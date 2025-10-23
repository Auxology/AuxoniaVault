using Auth.Application.Users.VerifyLogin;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class VerifyLogin : IEndpoint
{
    private sealed record Request(string Email, int Code);


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/verify-login", async (
            Request request,
            HttpContext httpContext,
            ISender sender) =>
        {
            var requestMetadata = httpContext.GetRequestMetadata();

            var command = new VerifyLoginCommand(request.Email, request.Code, requestMetadata);

            var result = await sender.Send(command);

            if (result.IsFailure)
                return CustomResults.Problem(result, httpContext);

            httpContext.SetAuthenticationCookie(result.Value.RefreshToken);

            return Results.Ok(new {accessToken = result.Value.AccessToken});
        })
        .WithTags(Tags.Users);
    }
}
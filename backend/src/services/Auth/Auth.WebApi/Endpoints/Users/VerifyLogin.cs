using Auth.Application.Users.VerifyLogin;
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
            var command = new VerifyLoginCommand(request.Email, request.Code);

            var result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
        })
        .WithTags(Tags.Users);
    }
}
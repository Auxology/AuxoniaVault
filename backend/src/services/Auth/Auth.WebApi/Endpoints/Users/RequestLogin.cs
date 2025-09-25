using Auth.Application.Users.RequestLogin;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class RequestLogin() : IEndpoint
{
    private sealed record Request(string Email);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/request-login", async (
            Request request,
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var command = new RequestLoginCommand(request.Email);

            var result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
        })
        .WithTags(Tags.Users);
    }
}
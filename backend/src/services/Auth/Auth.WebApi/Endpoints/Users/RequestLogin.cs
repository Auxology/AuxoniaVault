using Auth.Application.Users.RequestLogin;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class RequestLogin() : IEndpoint
{
    private sealed record Request(string Email);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/sessions/login-requests", async (
            Request request,
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var requestMetadata = httpContext.GetRequestMetadata();
            
            var command = new RequestLoginCommand(request.Email, requestMetadata);

            var result = await sender.Send(command);

            return result.IsSuccess ? Results.Accepted() : CustomResults.Problem(result, httpContext);
        })
        .WithTags(Tags.Users);
    }
}
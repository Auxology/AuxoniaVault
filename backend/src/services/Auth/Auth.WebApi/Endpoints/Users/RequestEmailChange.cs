using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Users.RequestEmailChange;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class RequestEmailChange : IEndpoint
{
    private sealed record Request(string Email);


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/email/request-change", async (
            Request request,
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var requestMetadata = httpContext.GetRequestMetadata();

            var command = new RequestEmailChangeCommand(request.Email, requestMetadata);

            var result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);

        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
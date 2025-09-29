using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Users.RequestEmailChange;
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
            string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString()
                               ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                               ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
                               ?? "unknown";

            string userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
            
            var requestMetadata = new RequestMetadata(ipAddress, userAgent);
            
            var command = new RequestEmailChangeCommand(request.Email, requestMetadata);

            var result = await sender.Send(command);
            
            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
            
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
using Auth.Application.Users.GetCurrentUser;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class GetCurrentUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/users/me", async
        (
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var query = new GetCurrentUserQuery();
            
            var result = await sender.Send(query, httpContext.RequestAborted);

            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
        })
        .WithTags(Tags.Users)
        .WithName("GetCurrentUser")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get current user info";
            operation.Description = "Retrieves information about the currently authenticated user.";
            return operation;
        })
        .RequireAuthorization();
    }
}
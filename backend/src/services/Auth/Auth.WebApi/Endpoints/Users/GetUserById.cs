using Auth.Application.Users.GetUser;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class GetUserById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/auth/users/{userId}", async (Guid userId, ISender sender, HttpContext context) =>
        {
            var query = new GetUserByIdQuery(userId);

            var result = await sender.Send(query);

            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, context);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
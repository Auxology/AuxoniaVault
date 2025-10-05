using Auth.Application.Users.ChangeName;
using Auth.SharedKernel;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class ChangeName : IEndpoint
{
    private sealed record Request(string Name);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/change-name", async
        (
            Request request,
            ISender sender,
            HttpContext httpContext
        ) =>
        {
            var command = new ChangeNameCommand(request.Name);

            Result<string> result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
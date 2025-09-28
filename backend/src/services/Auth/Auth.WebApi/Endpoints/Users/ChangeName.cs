using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class UpdateName : IEndpoint
{
    private sealed record Request(string Name);
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/change-name", async
        (
            Request request,
            ISender sender
        ) =>
        {
            var command = new ChangeName(request.Name);

            var result = await sender.Send(command);

            return .IsSuccess ? Results.Ok() : CustomResults.Problem(result);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
using Auth.Application.Users.SetProfilePicture;
using Auth.WebApi.Extensions;
using Auth.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class SetProfilePicture : IEndpoint
{
    private sealed record Request(IFormFile File);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/users/profile-picture/set", async
            (
                [FromForm] Request request,
                ISender sender,
                HttpContext context
            ) =>
            {
                var command = new SetProfilePictureCommand(request.File);

                var result = await sender.Send(command);

                return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, context);
            })
            .RequireAuthorization()
            .WithTags(Tags.Users)
            .DisableAntiforgery();
    }
}
using Main.Application.Files.DeleteFile;
using Main.WebApi.Infrastructure;
using MediatR;

namespace Main.WebApi.Endpoints.Files;

internal sealed class DeleteFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/files/{fileId:guid}", async
        (
            Guid fileId,
            HttpContext context,
            ISender sender
        ) =>
        {
            var command = new DeleteFileCommand(fileId);

            var result = await sender.Send(command);

            return result.IsSuccess ? Results.NoContent() : CustomResults.Problem(result, context);
        })
        .RequireAuthorization()
        .WithName("DeleteFile")
        .WithTags(Tags.Files)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Delete a file";
            operation.Description = "Deletes the specified file from the system.";
            return operation;
        });
    }
}
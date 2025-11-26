using Main.Application.Files.DownloadFile;
using Main.SharedKernel;
using Main.WebApi.Infrastructure;
using MediatR;

namespace Main.WebApi.Endpoints.Files;

internal sealed class DownloadFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/files/{fileId:guid}/download", async
        (
            Guid fileId,
            ISender sender,
            HttpContext httpContext
        ) =>
        {
            var command = new DownloadFileCommand(fileId);
            
            Result<string> result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithName("DownloadFile")
        .WithTags(Tags.Files)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get file download URL";
            operation.Description = "Retrieves a pre-signed URL to download the specified file.";
            return operation;
        });
    }
}
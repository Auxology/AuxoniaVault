using Main.Application.Files.StartMultipartUpload;
using Main.WebApi.Infrastructure;
using MediatR;

namespace Main.WebApi.Endpoints.Files;

internal sealed class StartMultipartUpload : IEndpoint
{
    private sealed record Request(string FileName, long FileSize, string ContentType);
    
    public async void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/files/start-multipart", async
        (
            Request request,
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var command = new StartMultipartUploadCommand
            (
                FileName: request.FileName,
                FileSize: request.FileSize,
                ContentType: request.ContentType
            );
            
            var result = await sender.Send(command);

            return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithName("StartMultipartUpload")
        .WithTags(Tags.Files)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Start a multipart file upload";
            operation.Description = "Initiates a multipart upload session for a file with the specified name, size, and content type";
            return operation;
        });    
    }
}
using Main.Application.Files.PresignedPartUpload;
using Main.WebApi.Infrastructure;
using MediatR;

namespace Main.WebApi.Endpoints.Files;

internal sealed class PresignedPartUpload : IEndpoint
{
    private sealed record Request(string UploadId, int PartNumber);
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/files/{key}/uploads/parts/presigned-urls", async
            (
                string key,
                Request request,
                HttpContext httpContext,
                ISender sender
            ) =>
            {
                var command = new PresignedPartUploadCommand
                (
                    FileKey: key,
                    UploadId: request.UploadId,
                    PartNumber: request.PartNumber
                );
            
                var result = await sender.Send(command);

                return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
            })
            .RequireAuthorization()
            .WithName("PresignedPartUpload")
            .WithTags(Tags.Files)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get presigned URL for uploading a file part";
                operation.Description = "Generates a presigned URL that allows the client to upload a specific part of a multipart file upload.";
                return operation;
            });    
    }
}
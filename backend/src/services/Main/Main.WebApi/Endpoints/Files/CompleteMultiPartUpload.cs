using Main.Application.Files.CompleteMultipartUpload;
using Main.WebApi.Infrastructure;
using MediatR;
using PartETag = Main.Domain.Objects.PartETag;

namespace Main.WebApi.Endpoints.Files;

internal sealed class CompleteMultipartUpload : IEndpoint
{
    private sealed record Request(
        string UploadId,
        List<PartETagDto> Parts,
        string FileName,
        long FileSizeInBytes,
        string ContentType
    );
    
    private sealed record PartETagDto(int PartNumber, string ETag);
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/files/{key}/uploads", async
            (
                string key,
                Request request,
                HttpContext httpContext,
                ISender sender
            ) =>
            {
                var command = new CompleteMultipartUploadCommand
                (
                    FileKey: key,
                    UploadId: request.UploadId,
                    Parts: request.Parts.Select(p => new PartETag(p.PartNumber, p.ETag)).ToList(),
                    FileName: request.FileName,
                    FileSizeInBytes: request.FileSizeInBytes,
                    ContentType: request.ContentType
                );
            
                var result = await sender.Send(command);

                return result.IsSuccess ? Results.Created($"/api/files/{key}", result.Value) : CustomResults.Problem(result, httpContext);
            })
            .RequireAuthorization()
            .WithName("CompleteMultipartUpload")
            .WithTags(Tags.Files)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Complete a multipart file upload";
                operation.Description = "Finalizes a multipart upload by assembling all uploaded parts into a single file";
                return operation;
            });    
    }
}
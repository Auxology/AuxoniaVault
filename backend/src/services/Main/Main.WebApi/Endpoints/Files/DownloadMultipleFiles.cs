using Main.Application.Files.DownloadMultipleFiles;
using Main.SharedKernel;
using Main.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Main.WebApi.Endpoints.Files;

internal sealed class DownloadMultipleFiles : IEndpoint
{
    private sealed record DownloadMultipleFilesRequest(List<Guid> FileIds);
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/files/download-multiple", async
        (
            DownloadMultipleFilesRequest request,
            ISender sender,
            HttpContext httpContext
        ) =>
        {
            DownloadMultipleFilesCommand command = new(request.FileIds);

            Result<DownloadMultipleFilesResponse> result = await sender.Send(command);

            if (result.IsFailure)
                return CustomResults.Problem(result, httpContext);

            if (result.Value.FailureCount > 0)
            {
                return Results.Json
                (
                    result.Value,
                    statusCode: StatusCodes.Status207MultiStatus
                );
            }

            return Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithName("DownloadMultipleFiles")
        .WithTags(Tags.Files)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Download multiple files";
            operation.Description = "Generates presigned download URLs for multiple files concurrently. Returns 207 Multi-Status if some files fail.";
            return operation;
        });
    }
}
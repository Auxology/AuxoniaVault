using Main.Application.Files.SearchFiles;
using Main.SharedKernel;
using Main.WebApi.Infrastructure;
using MediatR;

namespace Main.WebApi.Endpoints.Files;

internal sealed class SearchFiles : IEndpoint
{
    private sealed record Request(
        string? SearchTerm,
        List<string>? ContentTypes,
        bool? IsStarred,
        DateTimeOffset? CreatedAfter,
        DateTimeOffset? CreatedBefore,
        long? MinSizeInBytes,
        long? MaxSizeInBytes,
        int Page,
        int PageSize,
        string SortBy
    );
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/files/search", async
            (
                HttpContext httpContext,
                ISender sender,
                Request request
            ) =>
            {
                var command = new SearchFilesQuery
                (
                    SearchTerm: request.SearchTerm,
                    ContentTypes: request.ContentTypes,
                    IsStarred: request.IsStarred,
                    CreatedAfter: request.CreatedAfter,
                    CreatedBefore: request.CreatedBefore,
                    MinSizeInBytes: request.MinSizeInBytes,
                    MaxSizeInBytes: request.MaxSizeInBytes,
                    Page: request.Page,
                    PageSize: request.PageSize,
                    SortBy: request.SortBy
                );

                Result<SearchFilesResponse> result = await sender.Send(command);

                return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
            })
            .RequireAuthorization()
            .WithName("SearchFiles")
            .WithTags(Tags.Files)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search files with filters and pagination";
                operation.Description =
                    "Allows searching for files using various filters such as content type, date range, size, and supports pagination and sorting.";
                return operation;
            });
    }
}
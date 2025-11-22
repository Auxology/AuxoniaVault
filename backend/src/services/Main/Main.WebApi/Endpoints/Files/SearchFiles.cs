using Main.Application.Files.SearchFiles;
using Main.SharedKernel;
using Main.WebApi.Infrastructure;
using MediatR;

namespace Main.WebApi.Endpoints.Files;

internal sealed class SearchFiles : IEndpoint
{
    private sealed record Request(
        string? SearchTerm,
        string[]? ContentTypes,
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
        app.MapGet("/api/files", async
            (
                HttpContext httpContext,
                ISender sender,
                string? searchTerm,
                string[]? contentTypes,
                bool? isStarred,
                DateTimeOffset? createdAfter,
                DateTimeOffset? createdBefore,
                long? minSizeInBytes,
                long? maxSizeInBytes,
                int page = 1,
                int pageSize = 20,
                string sortBy = "createdAt"
            ) =>
            {
                var command = new SearchFilesQuery
                (
                    SearchTerm: searchTerm,
                    ContentTypes: contentTypes?.ToList(),
                    IsStarred: isStarred,
                    CreatedAfter: createdAfter,
                    CreatedBefore: createdBefore,
                    MinSizeInBytes: minSizeInBytes,
                    MaxSizeInBytes: maxSizeInBytes,
                    Page: page,
                    PageSize: pageSize,
                    SortBy: sortBy
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
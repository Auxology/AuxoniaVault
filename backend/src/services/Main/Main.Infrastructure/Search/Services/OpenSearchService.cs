using Main.Application.Abstractions.DTOs;
using Main.Domain.Aggregates.FileMetadata;
using Main.Infrastructure.Search.Errors;
using Main.Infrastructure.Search.Models;
using Main.Infrastructure.Search.Services.Helpers;
using Main.Infrastructure.Search.Settings;
using Main.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;
using Result = Main.SharedKernel.Result;

namespace Main.Infrastructure.Search.Services;

internal sealed class OpenSearchService(
    IOpenSearchClient openSearchClient,
    IOptions<OpenSearchSettings> settings,
    ILogger<OpenSearchService> logger) : IOpenSearchService
{
    private readonly string _indexName = settings.Value.DefaultIndex;
    
    public async Task<Result> InitializeIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var existsResponse = await openSearchClient.Indices.ExistsAsync(_indexName, ct: cancellationToken);
            
            if (existsResponse.Exists)
            {
                logger.LogInformation("OpenSearch index {IndexName} already exists", _indexName);
                return Result.Success();
            }
            
            var createIndexDescriptor = IndexMappings.GetFileMetadataIndexDescriptor(_indexName);
            var createResponse = await openSearchClient.Indices.CreateAsync(createIndexDescriptor, ct: cancellationToken);

            if (!createResponse.IsValid)
            {
                logger.LogError("Failed to create index {IndexName}: {Error}", _indexName, createResponse.DebugInformation);
                return Result.Failure(SearchErrors.IndexCreationFailed);
            }
            
            logger.LogInformation("Successfully created OpenSearch index {IndexName}", _indexName);
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error initializing OpenSearch index {IndexName}", _indexName);
            return Result.Failure(SearchErrors.UnexpectedError);
        }
    }

    public async Task<Result> IndexFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = OpenSearchHelpers.MapToDocument(fileMetadata);

            var response = await openSearchClient.IndexAsync(document, idx => idx
                .Index(_indexName)
                .Id(document.Id)
                .Refresh(Refresh.WaitFor), cancellationToken);

            if (!response.IsValid)
            {
                logger.LogError("Failed to index file metadata for FileId {FileId}: {Error}", fileMetadata.Id, response.DebugInformation);
                return Result.Failure(SearchErrors.IndexingFailed);
            }
            
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error indexing file metadata for FileId {FileId}", fileMetadata.Id);
            return Result.Failure(SearchErrors.UnexpectedError);
        }
    }

    public async Task<Result> UpdateFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
    {
        return await IndexFileMetadataAsync(fileMetadata, cancellationToken);
    }

    public async Task<Result> DeleteFileMetadataAsync(Guid fileMetadataId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await openSearchClient.DeleteAsync<FileMetadataDocument>(fileMetadataId.ToString(), del =>
                del
                    .Index(_indexName)
                    .Refresh(Refresh.WaitFor), cancellationToken);

            if (!response.IsValid)
            {
                logger.LogError("Failed to delete file metadata for FileId {FileId}: {Error}", fileMetadataId, response.DebugInformation);
                return Result.Failure(SearchErrors.DeletionFailed);
            }
            
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error deleting file metadata for FileId {FileId}", fileMetadataId);
            return Result.Failure(SearchErrors.UnexpectedError);
        }
    }

    public async Task<Result> BulkIndexFileMetadataAsync(IEnumerable<FileMetadata> fileMetadataList, CancellationToken cancellationToken = default)
    {
        try
        {
            List<FileMetadataDocument> documents = fileMetadataList.Select(OpenSearchHelpers.MapToDocument).ToList();

            var bulkResponse = await openSearchClient.BulkAsync(b => b
                .Index(_indexName)
                .Refresh(Refresh.WaitFor)
                .IndexMany(documents), cancellationToken);

            if (!bulkResponse.IsValid || bulkResponse.Errors)
            {
                logger.LogError("Bulk indexing had errors: {Error}", bulkResponse.DebugInformation);
                return Result.Failure(SearchErrors.IndexingFailed);
            }
            
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error bulk indexing file metadata");
            return Result.Failure(SearchErrors.UnexpectedError);
        }
    }

    public async Task<Result<SearchResult<FileMetadataSearchResult>>> SearchFilesAsync(string searchTerm, Guid ownerId,
        SearchFilters searchFilters, int page, int pageSize, string sortBy,
        CancellationToken cancellationToken = default)
    {
        var searchResponse = await openSearchClient.SearchAsync<FileMetadataDocument>(s => s
            .Index(_indexName)
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => OpenSearchHelpers.BuildFilesQuery(q, ownerId, searchTerm, searchFilters))
            .Sort(so => OpenSearchHelpers.BuildFileSort(so, sortBy))
            .Aggregations(OpenSearchHelpers.BuildFileAggregation), cancellationToken);

        if (!searchResponse.IsValid)
        {
            logger.LogError("Search query failed: {Error}", searchResponse.DebugInformation);
            return Result.Failure<SearchResult<FileMetadataSearchResult>>(SearchErrors.SearchFailed);
        }

        List<FileMetadataSearchResult> results = searchResponse.Documents.Select(doc =>
                OpenSearchHelpers.MapToSearchResult(doc, searchResponse.Hits.First(h => h.Id == doc.Id).Score ?? 0))
            .ToList();
        
        var aggregations = OpenSearchHelpers.ExtractAggregations(searchResponse.Aggregations);

        var searchResult = new SearchResult<FileMetadataSearchResult>
        (
            results,
            searchResponse.Total,
            aggregations
        );

        return Result.Success(searchResult);
    }

    public async Task<Result<IReadOnlyList<string>>> AutocompleteAsync(string searchTerm, Guid ownerId, int limit, CancellationToken cancellationToken = default)
    {
        try
        {
            var searchResponse = await openSearchClient.SearchAsync<FileMetadataDocument>(s => s
                .Index(_indexName)
                .Size(limit)
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Term(t => t.Field(f => f.OwnerId).Value(ownerId.ToString())),
                            m => m.Prefix(p => p.Field(f => f.FileName.Suffix("keyword")).Value(searchTerm))
                        )
                    )
                )
                .Source(src => src.Includes(i => i.Field(f => f.FileName))), cancellationToken);

            if (!searchResponse.IsValid)
            {
                logger.LogError("Autocomplete query failed for ownerId {OwnerId}: {Error}", ownerId, searchResponse.DebugInformation);
                return Result.Failure<IReadOnlyList<string>>(SearchErrors.SearchFailed);
            }
            
            var suggestions = searchResponse.Documents
                .Select(d => d.FileName)
                .Distinct()
                .ToList();

            return Result.Success<IReadOnlyList<string>>(suggestions);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error during autocomplete for ownerId {OwnerId}", ownerId);
            return Result.Failure<IReadOnlyList<string>>(SearchErrors.UnexpectedError);
        }
    }
}
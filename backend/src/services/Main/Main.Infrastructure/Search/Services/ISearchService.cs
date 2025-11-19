using Main.Domain.Aggregates.FileMetadata;
using Main.Infrastructure.Search.DTOs;
using Main.SharedKernel;

namespace Main.Infrastructure.Search.Services;

public interface ISearchService
{
    Task<Result> InitializeIndexAsync(CancellationToken cancellationToken = default);
    
    Task<Result> IndexFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default);
    
    Task<Result> UpdateFileMetadataAsync(FileMetadata fileMetadata, CancellationToken cancellationToken = default);
    
    Task<Result> DeleteFileMetadataAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<Result> BulkIndexFileMetadataAsync(IEnumerable<FileMetadata> fileMetadataList,
        CancellationToken cancellationToken = default);

    Task<Result<SearchResult<FileMetadataSearchResult>>> SearchFilesAsync
    (
        string searchTerm,
        Guid ownerId,
        SearchFilters searchFilters,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<string>> AutocompleteAsync
    (
        string searchTerm,
        Guid ownerId,
        int limit,
        CancellationToken cancellationToken = default
    );
}
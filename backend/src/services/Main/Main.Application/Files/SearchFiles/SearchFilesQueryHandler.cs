using Main.Application.Abstractions.Authentication;
using Main.Application.Abstractions.Database;
using Main.Application.Abstractions.DTOs;
using Main.Application.Abstractions.Messaging;
using Main.Application.Errors;
using Main.Domain.Aggregates.Account;
using Main.Domain.ValueObjects;
using Main.Infrastructure.Search.Services;
using Main.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Files.SearchFiles;

internal sealed class SearchFilesQueryHandler(IMainDbContext context, IUserContext userContext, IOpenSearchService searchService) : IQueryHandler<SearchFilesQuery, SearchFilesResponse> 
{
    public async Task<Result<SearchFilesResponse>> Handle(SearchFilesQuery request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);
        
        Account? account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, cancellationToken);
        
        if (account is null)
            return Result.Failure<SearchFilesResponse>(AccountErrors.NotFound);
        
        var filters = new SearchFilters
        (
            ContentTypes: request.ContentTypes,
            IsStarred: request.IsStarred,
            CreatedAfter: request.CreatedAfter,
            CreatedBefore: request.CreatedBefore,
            MinFileSizeInBytes: request.MinSizeInBytes,
            MaxFileSizeInBytes: request.MaxSizeInBytes
        );

        Result<SearchResult<FileMetadataSearchResult>> searchResult = await searchService.SearchFilesAsync
        (
            searchTerm: request.SearchTerm ?? string.Empty,
            ownerId: userId.Value,
            searchFilters: filters,
            page: request.Page,
            pageSize: request.PageSize,
            sortBy: request.SortBy,
            cancellationToken: cancellationToken
        );
        
        if (searchResult.IsFailure)
            return Result.Failure<SearchFilesResponse>(searchResult.Error);

        SearchResult<FileMetadataSearchResult> result = searchResult.Value;
        
        List<FileMetadataDto> files = result.Items.Select(item => new FileMetadataDto
        (
            Id: item.Id,
            FileName: item.FileName,
            ContentType: item.ContentType,
            FileSizeInBytes: item.SizeInBytes,
            FileKey: item.FileKey,
            CreatedAt: item.CreatedAt,
            ModifiedAt: item.ModifiedAt,
            Description: item.Description,
            IsStarred: item.IsStarred,
            Score: item.Score
        )).ToList();
        
        var response = new SearchFilesResponse
        (
            Files: files,
            TotalCount: result.TotalCount,
            Page: request.Page,
            PageSize: request.PageSize,
            ContentTypeFacets: result.Aggregations.ContentTypeCounts,
            DateHistogram: result.Aggregations.DateHistogram
        );
        
        return Result.Success(response);
    }
}
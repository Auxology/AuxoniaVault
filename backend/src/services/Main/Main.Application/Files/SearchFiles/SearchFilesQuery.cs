using Main.Application.Abstractions.Messaging;

namespace Main.Application.Files.SearchFiles;

public sealed record SearchFilesQuery
(
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
) : IQuery<SearchFilesResponse>;
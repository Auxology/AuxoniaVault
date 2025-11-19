namespace Main.Application.Files.SearchFiles;

public sealed record SearchFilesResponse
(
    IReadOnlyList<FileMetadataDto> Files,
    long TotalCount,
    int Page,
    int PageSize,
    Dictionary<string, long> ContentTypeFacets,
    Dictionary<string, long> DateHistogram
);

public sealed record FileMetadataDto
(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeInBytes,
    string FileKey,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt,
    string? Description,
    bool IsStarred,
    double Score
);
namespace Main.Application.Abstractions.DTOs;

public sealed record FileMetadataSearchResult
(
    Guid Id,
    Guid OwnerId,
    string FileName,
    string ContentType,
    long SizeInBytes,
    string FileKey,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt,
    string? Description,
    bool IsStarred,
    double Score
);
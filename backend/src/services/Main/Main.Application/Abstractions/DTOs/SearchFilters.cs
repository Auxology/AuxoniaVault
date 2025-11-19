namespace Main.Application.Abstractions.DTOs;

public sealed record SearchFilters
(
    IReadOnlyList<string>? ContentTypes = null,
    bool? IsStarred = null,
    DateTimeOffset? CreatedAfter = null,
    DateTimeOffset? CreatedBefore = null,
    long? MinFileSizeInBytes = null,
    long? MaxFileSizeInBytes = null
);
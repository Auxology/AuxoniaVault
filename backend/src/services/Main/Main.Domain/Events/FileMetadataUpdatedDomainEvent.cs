using Main.SharedKernel;

namespace Main.Domain.Events;

public sealed record FileMetadataUpdatedDomainEvent
(
    Guid FileId,
    Guid OwnerId,
    string FileName,
    string? FileDescription,
    bool IsStarred,
    DateTimeOffset ModifiedAt
) : IDomainEvent;
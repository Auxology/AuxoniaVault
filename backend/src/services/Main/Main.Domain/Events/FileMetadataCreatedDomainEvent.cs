using Main.SharedKernel;

namespace Main.Domain.Events;

public sealed record FileMetadataCreatedDomainEvent
(
    Guid FileId,
    Guid OwnerId,
    string FileName,
    string ContentType,
    long FileSizeInBytes,
    string FileKey,
    DateTimeOffset CreatedAt
) : IDomainEvent;
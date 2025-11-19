using Main.SharedKernel;

namespace Main.Domain.Events;

public sealed record FileMetadataDeletedDomainEvent
(
    Guid FileId,
    Guid OwnerId,
    DateTimeOffset DeletedAt
) : IDomainEvent;
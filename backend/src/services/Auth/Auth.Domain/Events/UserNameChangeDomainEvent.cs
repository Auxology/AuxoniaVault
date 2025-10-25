using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record UserNameChangeDomainEvent
(
    Guid UserId,
    string NewName,
    DateTimeOffset ChangedAt
) : IDomainEvent;
using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record EmailChangedDomainEvent
(
    Guid UserId,
    string NewEmail,
    DateTimeOffset ChangedAt
) : IDomainEvent;
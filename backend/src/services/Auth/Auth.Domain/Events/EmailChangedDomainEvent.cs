using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record EmailChangedDomainEvent
(
    Guid UserId,
    string NewEmail,
    string IpAddress,
    string UserAgent,
    DateTimeOffset ChangedAt
) : IDomainEvent, IAuditLoggedDomainEvent;
using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record EmailChangeRequestedDomainEvent
(
    Guid UserId,
    string CurrentEmail,
    int CurrentOtp,
    string IpAddress,
    string UserAgent,
    DateTimeOffset RequestedAt
) : IDomainEvent, IAuditLoggedDomainEvent;
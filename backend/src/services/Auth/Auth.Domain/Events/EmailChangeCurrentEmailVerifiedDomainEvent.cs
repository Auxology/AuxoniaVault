using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record EmailChangeCurrentEmailVerifiedDomainEvent
(
    Guid UserId,
    string NewEmail,
    int NewOtp,
    string IpAddress,
    string UserAgent,
    DateTimeOffset RequestedAt
) : IDomainEvent, IAuditLoggedDomainEvent;
using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record UserRecoveredDomainEvent
(
    Guid UserId,
    string NewEmail,
    string IpAddress,
    string UserAgent,
    DateTimeOffset RecoveredAt
) : IDomainEvent;
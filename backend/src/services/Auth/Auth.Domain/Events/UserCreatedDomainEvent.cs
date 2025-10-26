using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record UserCreatedDomainEvent
(
    Guid UserId,
    string Email,
    string Name,
    string IpAddress,
    string UserAgent,
    DateTimeOffset CreatedAt
) : IDomainEvent, IAuditLoggedDomainEvent;
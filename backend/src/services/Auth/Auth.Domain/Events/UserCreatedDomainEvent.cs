using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record UserCreatedDomainEvent
(
    Guid UserId,
    string Email,
    string Name,
    DateTimeOffset CreatedAt
) : IDomainEvent;
using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record LoginRequestedDomainEvent
(
    string Email,
    int Token,
    DateTimeOffset RequestedAt
) : IDomainEvent;
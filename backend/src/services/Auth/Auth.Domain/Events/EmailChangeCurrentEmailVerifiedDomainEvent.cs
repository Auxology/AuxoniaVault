using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record EmailChangeCurrentEmailVerifiedDomainEvent
(
    string NewEmail,
    int NewOtp,
    DateTimeOffset RequestedAt
) : IDomainEvent;
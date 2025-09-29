using Auth.SharedKernel;

namespace Auth.Domain.Events;

public sealed record EmailChangeRequestedDomainEvent
(
    string CurrentEmail,
    int CurrentOtp,
    string IpAddress,
    string UserAgent,
    DateTimeOffset RequestedAt
) : IDomainEvent;
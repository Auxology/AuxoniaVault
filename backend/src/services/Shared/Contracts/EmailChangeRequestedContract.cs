namespace Shared.Contracts;

public sealed record EmailChangeRequestedContract
(
    string CurrentEmail,
    int CurrentOtp,
    string IpAddress,
    string UserAgent,
    DateTimeOffset RequestedAt
);
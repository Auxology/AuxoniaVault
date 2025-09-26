namespace Shared.Contracts;

public sealed record LoginRequestedContract
(
    string Email,
    int Token,
    DateTimeOffset RequestedAt
);
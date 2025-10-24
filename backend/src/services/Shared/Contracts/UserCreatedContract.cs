namespace Shared.Contracts;

public sealed record UserCreatedContract
(
    Guid UserId,
    string Email,
    string Name,
    DateTimeOffset CreatedAt
);
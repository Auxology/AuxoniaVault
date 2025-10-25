namespace Shared.Contracts;

public sealed record UserNameChangedContract
(
    Guid UserId,
    string NewName,
    DateTimeOffset ChangedAt
);
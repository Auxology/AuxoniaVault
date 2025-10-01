namespace Shared.Contracts;

public sealed record EmailChangedContract
(
    Guid UserId,
    string NewEmail,
    DateTimeOffset ChangedAt
);

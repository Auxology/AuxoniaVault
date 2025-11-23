namespace Shared.Contracts;

public sealed record UserRecoveredContract
(
    Guid UserId,
    string NewEmail,
    string IpAddress,
    string UserAgent,
    DateTimeOffset RecoveredAt
);
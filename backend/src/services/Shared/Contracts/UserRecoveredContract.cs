namespace Shared.Contracts;

public sealed class UserRecoveredContract
(
    Guid UserId,
    string NewEmail,
    string IpAddress,
    string UserAgent,
    DateTimeOffset RecoveredAt
);
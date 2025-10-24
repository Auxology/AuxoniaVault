namespace Auth.Domain.Aggregates.AuditLog;

public enum AuditAction
{
    UserCreated,
    LoginRequested,
    LoginVerified,
    EmailChangeRequested,
    EmailChangeCurrentEmailVerified,
    EmailChanged,
    NameChanged,
    ProfilePictureSet,
    RecoveryCodesGenerated,
    RecoveryRequested,
    RecoveryCompleted,
    SessionCreated,
    SessionRevoked
}
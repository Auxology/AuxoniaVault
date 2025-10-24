using Auth.SharedKernel;

namespace Auth.Domain.Errors;

internal static class AuditLogErrors
{
    public static Error UserIdRequired => Error.Validation(
        "AuditLog.UserIdRequired",
        "UserId is required for audit log entry."
    );

    public static Error EntityTypeRequired => Error.Validation(
        "AuditLog.EntityTypeRequired",
        "EntityType is required for audit log entry."
    );

    public static Error EntityIdRequired => Error.Validation(
        "AuditLog.EntityIdRequired",
        "EntityId is required for audit log entry."
    );
}
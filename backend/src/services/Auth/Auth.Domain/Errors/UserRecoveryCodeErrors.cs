using Auth.SharedKernel;

namespace Auth.Domain.Errors;

internal static class UserRecoveryCodeErrors
{
    public static Error UserIdRequired = Error.Validation
    (
        code: "UserRecoveryCodeErrors.UserIdRequired",
        description: "The user ID is required."
    );
    
    public static Error HashedCodeRequired = Error.Validation
    (
        code: "UserRecoveryCodeErrors.HashedCodeRequired",
        description: "The hashed code is required."
    );
    
    public static Error HashedCodesRequired = Error.Validation
    (
        code: "UserRecoveryCodeErrors.HashedCodesRequired",
        description: "The hashed codes are required."
    );
}
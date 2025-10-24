using Auth.SharedKernel;

namespace Auth.Application.Errors;

internal static class UserErrors
{
    public static Error EmailNotUnique => Error.Conflict
    (
        "Users.EmailNotUnique",
        "The provided email address is already in use."
    );

    public static Error UserNotFound => Error.NotFound
    (
        "Users.NotFound",
        "The specified user was not found."
    );

    public static Error Unauthorized() => Error.Unauthorized
    (
        "Users.Unauthorized",
        "You are not authorized to perform this action."
    );
    
    public static Error InvalidRecoveryCode => Error.Validation
    (
        "Users.RecoveryCodeInvalid",
        "The provided recovery code is invalid."
    );
    
    public static Error NoRecoveryCodesAvailable => Error.Validation
    (
        "Users.NoRecoveryCodesAvailable",
        "No recovery codes are available for this user."
    );
}
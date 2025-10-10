using Auth.Domain.Constants;
using Auth.SharedKernel;

namespace Auth.Domain.Errors;

internal static class UserErrors
{
    public static Error NameRequired => Error.Validation
    (
        "Users.NameRequired",
        "The name is required."
    );

    public static Error NameTooLong => Error.Validation
    (
        "Users.NameTooLong",
        "The name must not exceed 256 characters."
    );

    public static Error EmailCannotBeSame => Error.Validation
    (
        "Users.EmailCannotBeSame",
        "The new email address cannot be the same as the current one."
    );

    public static Error AvatarKeyRequired => Error.Validation
    (
        "Users.AvatarKeyRequired",
        "The avatar key is required."
    );

    public static Error RecoveryCodesExceedLimit => Error.Validation
    (
        "Users.RecoveryCodesExceedLimit",
        $"The number of recovery codes exceeds the limit of {UserConstants.MaxRecoveryCodes}"
    );
    
    public static Error RecoveryCodesAlreadyExist => Error.Conflict
    (
        "Users.RecoveryCodesAlreadyExist",
        "The user already has recovery codes."
    );
}
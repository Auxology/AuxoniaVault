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
}
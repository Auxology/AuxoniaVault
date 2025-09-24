using Auth.SharedKernel;

namespace Auth.Application.Errors;

internal static class UserErrors
{
    public static Error EmailNotUnique => Error.Conflict
    (
        "User.EmailNotUnique",
        "The provided email address is already in use."
    );
}
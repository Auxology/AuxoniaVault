using Auth.SharedKernel;

namespace Auth.Application.Errors;

internal static class UserRecoveryErrors
{
    public static Error RequestNotFound => Error.NotFound
    (
        "UserRecovery.RequestNotFound",
        "The specified user recovery request was not found."
    );
}
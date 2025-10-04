using Auth.SharedKernel;

namespace Auth.Application.Errors;

internal static class SessionErrors
{
    public static Error RefreshTokenExpired => Error.Unauthorized
    (
        "Sessions.RefreshTokenExpired",
        "The provided refresh token is expired."
    );

    public static Error UserNotFound => Error.NotFound
    (
        "Sessions.UserNotFound",
        "The user associated with the session was not found."
    );

    public static Error SessionNotFound => Error.NotFound
    (
        "Sessions.SessionNotFound",
        "The session with the provided refresh token was not found."
    );

    public static Error UnauthorizedAccess => Error.Unauthorized
    (
        "Sessions.UnauthorizedAccess",
        "You do not have permission to revoke sessions for this user."
    );
}
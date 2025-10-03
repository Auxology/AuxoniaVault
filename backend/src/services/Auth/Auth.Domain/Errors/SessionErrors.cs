using Auth.SharedKernel;

namespace Auth.Domain.Errors;

internal static class SessionErrors
{
    public static Error UserIdRequired => Error.Validation
    (
        "Sessions.UserIdRequired",
        "UserId is required."
    );
    
    public static Error TokenRequired => Error.Validation
    (
        "Sessions.TokenRequired",
        "Token is required."
    );
    
    public static Error InfoRequired => Error.Validation
    (
        "Sessions.InfoRequired",
        "IP address and User Agent are required."
    );

    public static Error SessionInvalid => Error.Unauthorized
    (
        "Sessions.SessionInvalid",
        "The provided refresh token is invalid or has been revoked."
    );

    public static Error RefreshTokenExpired => Error.Unauthorized
    (
        "Sessions.RefreshTokenExpired",
        "The provided refresh token is expired."
    );

    public static Error SessionNotFound => Error.NotFound
    (
        "Sessions.SessionNotFound",
        "The session with the provided refresh token was not found."
    );
}
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
}
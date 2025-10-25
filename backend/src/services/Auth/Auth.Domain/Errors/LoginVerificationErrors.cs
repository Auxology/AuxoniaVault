using Auth.SharedKernel;

namespace Auth.Domain.Errors;

internal static class LoginVerificationErrors
{
    public static Error InvalidValue => Error.Validation
    (
        "LoginVerifications.InvalidValue",
        "The login verification value is invalid."
    );

    public static Error InvalidIpAddress => Error.Validation
    (
        "LoginVerifications.InvalidIpAddress",
        "The IP address is invalid."
    );
    
    public static Error InvalidUserAgent => Error.Validation
    (
        "LoginVerifications.InvalidUserAgent",
        "The user agent is invalid."
    );
}
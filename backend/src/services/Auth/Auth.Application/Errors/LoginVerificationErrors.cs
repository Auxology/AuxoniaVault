using Auth.SharedKernel;

namespace Auth.Application.Errors;

internal static class LoginVerificationErrors
{
    public static Error InvalidOrExpired => Error.Validation
    (
        code: "LoginVerifications.InvalidOrExpired",
        description: "The provided login verification code is invalid or has expired."
    );
}
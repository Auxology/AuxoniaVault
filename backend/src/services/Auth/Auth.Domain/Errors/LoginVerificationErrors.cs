using Auth.SharedKernel;

namespace Auth.Domain.Errors;

internal static class LoginVerificationErrors
{
    public static Error InvalidValue => Error.Validation
    (
        "LoginVerifications.InvalidValue",
        "The login verification value is invalid."
    );
}
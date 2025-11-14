using Gateway.SharedKernel;

namespace Gateway.Infrastructure.Errors;

internal static class AuthErrors
{
    public static Error FailedToDeserialize => Error.Failure
    (
        "AuthErrors.FailedToDeserialize",
        "Failed to deserialize the response from the authentication service."
    );
}
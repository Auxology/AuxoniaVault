using Auth.SharedKernel;

namespace Auth.Application.Errors;

internal static class EmailChangeRequestErrors
{
    public static Error NotFound => Error.NotFound
    (
        "EmailChangeRequests.NotFound",
        "No pending email change request found."
    );
}
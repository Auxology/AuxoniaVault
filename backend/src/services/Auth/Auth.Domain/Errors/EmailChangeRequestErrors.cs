using Auth.SharedKernel;

namespace Auth.Domain.Errors;

internal static class EmailChangeRequestErrors
{
    public static Error UserIdRequired = Error.Validation
    (
        code: "EmailChangeRequests.UserIdRequired",
        description: "The user ID is required."
    );
    
    public static Error NewCannotBeSameAsCurrent = Error.Validation(
        code: "EmailChangeRequests.NewCannotBeSameAsCurrent",
        description: "The new email address cannot be the same as the current email address."
    );
    
    public static Error ActiveRequestExists => Error.Conflict
    (
        code: "EmailChangeRequests.ActiveRequestExists",
        description: "An active email change request already exists for this user."
    );
    
    public static Error InvalidOtp => Error.Validation
    (
        code: "EmailChangeRequests.InvalidOtp",
        description: "The provided OTP is invalid or expired."
    );
    
    public static Error InvalidStep => Error.Validation
    (
        code: "EmailChangeRequests.InvalidStep",
        description: "The email change request is not in the correct step for this operation."
    );
    
    public static Error NotFound => Error.NotFound
    (
        code: "EmailChangeRequests.NotFound",
        description: "No active email change request found."
    );
}
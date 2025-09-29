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
        description: "The provided OTP is invalid."
    );
    
    public static Error InvalidStepForCurrentOtp => Error.Validation
    (
        code: "EmailChangeRequests.InvalidStepForCurrentOtp",
        description: "The email change request is not in the correct step for this operation."
    );
}
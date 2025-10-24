using Auth.SharedKernel;

namespace Auth.Domain.Errors;

public static class UserRecoveryRequestErrors
{
    public static Error UserIdRequired = Error.Validation
    (
        code: "UserRecoveryRequests.UserIdRequired",
        description: "The user ID is required."
    );
    
    public static Error UniqueIdentifierRequired = Error.Validation
    (
        code: "UserRecoveryRequests.UniqueIdentifierRequired",
        description: "The Unique identifier is required."
    );

    public static Error AlreadyCompleted = Error.Unauthorized
    (
        "UserRecoveryRequests.AlreadyCompleted",
        "This recovery request has already been completed."
    );
    
    public static Error Expired = Error.Unauthorized
    (
        "UserRecoveryRequests.Expired",
        "This recovery request has expired."
    );
}
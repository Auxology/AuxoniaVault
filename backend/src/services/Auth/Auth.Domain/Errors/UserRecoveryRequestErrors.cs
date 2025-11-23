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

    public static Error AlreadyCompleted = Error.Conflict
    (
        "UserRecoveryRequests.AlreadyCompleted",
        "This recovery request has already been completed."
    );
    
    public static Error Expired = Error.Unauthorized
    (
        "UserRecoveryRequests.Expired",
        "This recovery request has expired."
    );
    
    public static Error IpAddressRequired = Error.Validation
    (
        code: "UserRecoveryRequests.IpAddressRequired",
        description: "The IP address is required."
    );
    
    public static Error UserAgentRequired = Error.Validation
    (
        code: "UserRecoveryRequests.UserAgentRequired",
        description: "The user agent is required."
    );
}
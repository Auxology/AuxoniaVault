namespace Auth.Domain.Constants;

public static class UserConstants
{
    public const int MaxNameLength = 256;

    public const int MaxEmailLength = 256;
    
    public const int MaxRecoveryCodes = 5;
    
    public const int MaxActiveRecoveryRequests = 3;
}
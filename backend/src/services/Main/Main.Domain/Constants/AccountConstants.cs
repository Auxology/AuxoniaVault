namespace Main.Domain.Constants;

public static class AccountConstants
{
    public const int FreeTier = 0;
    public const int ProTier = 1;
    
    public const long MaxStorageInBytesFreeTier = 5L * 1024 * 1024 * 1024; // 5 GB
    public const long MaxStorageInBytesProTier = 10L * 1024 * 1024 * 1024; // 10 GB
}
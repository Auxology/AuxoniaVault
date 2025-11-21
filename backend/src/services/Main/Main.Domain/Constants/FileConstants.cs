namespace Main.Domain.Constants;

public static class FileConstants
{
    public const int MinAllowedPartNumber = 1;
    public const int MaxAllowedPartNumber = 10000;
    
    public const int MaxFileNameLength = 500;
    public const int MaxDescriptionLength = 1000;
    
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;
    
    public const int MinAllowedFileSizeInBytes = 1;
    
    public const int MinSearchTermLength = 1;
    public const int MaxSearchTermLength = 100;
    
    public const int MinAutocompleteLimit = 1;
    public const int MaxAutocompleteLimit = 10;
}
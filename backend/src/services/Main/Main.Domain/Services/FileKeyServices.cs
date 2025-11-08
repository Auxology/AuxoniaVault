using Main.Domain.ValueObjects;
using Main.SharedKernel;

namespace Main.Domain.Services;

public static class FileKeyServices
{
    public static string CreateFileKey(string userId, string fileName, IDateTimeProvider dateTimeProvider)
    {
        var fileId = Guid.NewGuid();
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        string extension = Path.GetExtension(fileName);
        
        return $"{userId}/{utcNow:yyyy/MM/dd}/{fileId}{extension}";
    }
    
    public static string CreateFileKeyWithoutExtension(string userId, IDateTimeProvider dateTimeProvider)
    {
        var fileId = Guid.NewGuid();
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        return $"{userId}/{utcNow:yyyy/MM/dd}/{fileId}";
    }
}
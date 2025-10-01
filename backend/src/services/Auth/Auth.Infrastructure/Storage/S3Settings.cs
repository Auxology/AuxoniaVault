namespace Auth.Infrastructure.Storage;

internal sealed class S3Settings
{
    public string RegionName { get; init; } = string.Empty;
    
    public string BucketName { get; init; } = string.Empty;
}
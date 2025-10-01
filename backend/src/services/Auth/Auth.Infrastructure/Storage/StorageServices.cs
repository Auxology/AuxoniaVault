using Amazon.S3;
using Auth.Application.Abstractions.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth.Infrastructure.Storage;

internal sealed class StorageServices(IAmazonS3 amazonS3, IOptions<S3Settings> options, ILogger<StorageServices> logger) : IStorageServices
{
    private const string UserProfilePictures = "images/users/profile-pictures";
    
    public async Task<string> PutObjectAsync(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = file.OpenReadStream();

            var key = $"{UserProfilePictures}/{Guid.NewGuid():N}";

            var putObjectRequest = new Amazon.S3.Model.PutObjectRequest
            {
                BucketName = options.Value.BucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType,
                Metadata =
                {
                    ["file-name"] = file.FileName
                }
            };
        
            await amazonS3.PutObjectAsync(putObjectRequest, cancellationToken);

            return key;
        }
        
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while uploading the file to S3.");
            
            throw new Exception("An error occurred while uploading the file to S3.");
        }
    }
}
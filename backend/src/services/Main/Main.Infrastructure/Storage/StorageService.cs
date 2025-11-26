using Amazon.S3;
using Amazon.S3.Model;
using Main.Application.Abstractions.Storage;
using Main.Domain.Services;
using Main.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartETag = Main.Domain.Objects.PartETag;

namespace Main.Infrastructure.Storage;

internal sealed class StorageServices(IAmazonS3 amazonS3, IOptions<S3Settings> options, ILogger<StorageServices> logger, IDateTimeProvider dateTimeProvider) : IStorageServices
{
    private const string UserFiles = "users/files";
    private const int ExpiresInMinutes = 15;
    
    public async Task<Result<StartS3Response>> StartMultiPartUploadAsync(string userId, string fileName, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            var key = FileKeyServices.CreateFileKeyWithoutExtension(userId, dateTimeProvider);
            
            var formatedKey = $"{UserFiles}/{key}";

            var request = new InitiateMultipartUploadRequest
            {
                BucketName = options.Value.BucketName,
                Key = formatedKey,
                ContentType = contentType,
                Metadata =
                {
                    ["file-name"] = fileName
                }
            };
            
            var initiateResponse = await amazonS3.InitiateMultipartUploadAsync(request, cancellationToken);
            
            if (string.IsNullOrWhiteSpace(initiateResponse.UploadId))
            {
                logger.LogError("Failed to initiate multipart upload for user {UserId}", userId);
                return Result.Failure<StartS3Response>(StorageErrors.UploadFailed);
            }

            var serviceResponse = new StartS3Response
            (
                FileKey: key,
                UploadId: initiateResponse.UploadId
            );
            
            return Result.Success(serviceResponse);
        }
        catch (AmazonS3Exception s3Exception)
        {
            logger.LogError(s3Exception, "S3 error starting multipart upload for user {UserId}", userId);
            return Result.Failure<StartS3Response>(StorageErrors.UploadFailed);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error starting multipart upload for user {UserId}", userId);
            return Result.Failure<StartS3Response>(StorageErrors.UnexpectedError);
        }
    }

    public async Task<Result<string>> GetPresignedUrlAsync(string userId, string fileKey, string uploadId, int partNumber, CancellationToken cancellationToken)
    {
        try
        {
            var formatedKey = $"{UserFiles}/{fileKey}";
            
            DateTimeOffset utcNow = dateTimeProvider.UtcNow.AddMinutes(ExpiresInMinutes);
            
            var request = new GetPreSignedUrlRequest
            {
                BucketName = options.Value.BucketName,
                Key = formatedKey,
                Verb = HttpVerb.PUT,
                Expires = dateTimeProvider.ToDateTime(utcNow),
                UploadId = uploadId,
                PartNumber = partNumber
            };
            
            string? preSignedUrl = await amazonS3.GetPreSignedURLAsync(request);
            
            if (string.IsNullOrEmpty(preSignedUrl))
            {
                logger.LogError("Failed to generate presigned URL for user {UserId}, fileKey {FileKey}, uploadId {UploadId}, partNumber {PartNumber}", userId, fileKey, uploadId, partNumber);
                return Result.Failure<string>(StorageErrors.UploadFailed);
            }
            
            return Result.Success(preSignedUrl);
        }
        catch (AmazonS3Exception s3Exception)
        {
            logger.LogError(s3Exception, "S3 error generating presigned URL for user {UserId}", userId);
            return Result.Failure<string>(StorageErrors.UploadFailed);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error generating presigned URL for user {UserId}", userId);
            return Result.Failure<string>(StorageErrors.UnexpectedError);
        }
    }

    public async Task<Result<string>> CompleteMultiPartUploadAsync(string userId, string fileKey, string uploadId, List<PartETag> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var formatedKey = $"{UserFiles}/{fileKey}";
            
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = options.Value.BucketName,
                Key = formatedKey,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new Amazon.S3.Model.PartETag(p.PartNumber, p.ETag)).ToList()
            };
            
            var completeMultipartUploadResponse = await amazonS3.CompleteMultipartUploadAsync(request, cancellationToken);

            if (string.IsNullOrWhiteSpace(completeMultipartUploadResponse.Key))
            {
                logger.LogError("Failed to complete multipart upload for user {UserId}, fileKey {FileKey}, uploadId {UploadId}", userId, fileKey, uploadId);
                return Result.Failure<string>(StorageErrors.UploadFailed);
            }
            
            return Result.Success(fileKey);
        }
        catch (AmazonS3Exception s3Exception)
        {
            logger.LogError(s3Exception, "S3 error completing multipart upload for user {UserId}", userId);
            return Result.Failure<string>(StorageErrors.UploadFailed);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error completing multipart upload for user {UserId}", userId);
            return Result.Failure<string>(StorageErrors.UnexpectedError);
        }
    }

    public async Task<Result> RemoveFileAsync(string fileKey, CancellationToken cancellationToken)
    {
        var formatedKey = $"{UserFiles}/{fileKey}";
        
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = options.Value.BucketName,
                Key = formatedKey
            };
            
            await amazonS3.DeleteObjectAsync(request, cancellationToken);
            
            return Result.Success();
        }
        catch (AmazonS3Exception s3Exception)
        {
            logger.LogError(s3Exception, "S3 error removing file with key {FileKey}", fileKey);
            return Result.Failure(StorageErrors.DeletionFailed);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error removing file with key {FileKey}", fileKey);
            return Result.Failure(StorageErrors.UnexpectedError);
        }
    }

    public async Task<Result<string>> GetDownloadUrlAsync(string fileKey, CancellationToken cancellationToken)
    {
        var formatedKey = $"{UserFiles}/{fileKey}";

        DateTimeOffset utcNow = dateTimeProvider.UtcNow.AddMinutes(ExpiresInMinutes);

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = options.Value.BucketName,
                Key = formatedKey,
                Verb = HttpVerb.GET,
                Expires = dateTimeProvider.ToDateTime(utcNow)
            };

            string? preSignedUrl = await amazonS3.GetPreSignedURLAsync(request);

            if (string.IsNullOrEmpty(preSignedUrl))
            {
                logger.LogError("Failed to generate download URL for fileKey {FileKey}", fileKey);
                return Result.Failure<string>(StorageErrors.DownloadFailed);
            }

            return Result.Success(preSignedUrl);
        }
        catch (AmazonS3Exception s3Exception)
        {
            logger.LogError(s3Exception, "S3 error generating download URL for fileKey {FileKey}", fileKey);
            return Result.Failure<string>(StorageErrors.DownloadFailed);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error generating download URL for fileKey {FileKey}", fileKey);
            return Result.Failure<string>(StorageErrors.UnexpectedError);
        }
    }

    public async Task<Result<Dictionary<string, string>>> GetDownloadUrlsAsync(IEnumerable<string> fileKeys, CancellationToken cancellationToken)
    {
        List<string> keyList = fileKeys.ToList();

        if (keyList.Count == 0)
        {
            return Result.Success(new Dictionary<string, string>());
        }

        DateTimeOffset utcNow = dateTimeProvider.UtcNow.AddMinutes(ExpiresInMinutes);

        try
        {
            // Throttle concurrent S3 API calls to avoid rate limiting
            SemaphoreSlim throttle = new(maxCount: 10, initialCount: 10);

            List<Task<KeyValuePair<string, string>>> tasks = keyList
                .Select(async fileKey =>
                {
                    await throttle.WaitAsync(cancellationToken);
                    try
                    {
                        string formatedKey = $"{UserFiles}/{fileKey}";

                        GetPreSignedUrlRequest request = new()
                        {
                            BucketName = options.Value.BucketName,
                            Key = formatedKey,
                            Verb = HttpVerb.GET,
                            Expires = dateTimeProvider.ToDateTime(utcNow)
                        };

                        string? url = await amazonS3.GetPreSignedURLAsync(request);

                        return new KeyValuePair<string, string>(fileKey, url ?? string.Empty);
                    }
                    finally
                    {
                        throttle.Release();
                    }
                })
                .ToList();

            KeyValuePair<string, string>[] results = await Task.WhenAll(tasks);

            Dictionary<string, string> urlMap = results
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return Result.Success(urlMap);
        }
        catch (AmazonS3Exception s3Exception)
        {
            logger.LogError(s3Exception, "S3 error generating batch download URLs");
            return Result.Failure<Dictionary<string, string>>(StorageErrors.DownloadFailed);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error generating batch download URLs");
            return Result.Failure<Dictionary<string, string>>(StorageErrors.UnexpectedError);
        }
    }

    public async Task<Result<Stream>> GetFileStreamAsync(string fileKey, CancellationToken cancellationToken)
    {
        try
        {
            var formatedKey = $"{UserFiles}/{fileKey}";

            var request = new GetObjectRequest
            {
                BucketName = options.Value.BucketName,
                Key = formatedKey
            };
            
            GetObjectResponse response = await amazonS3.GetObjectAsync(request, cancellationToken);

            if (response.ResponseStream is null)
            {
                logger.LogError("Failed to get file stream for fileKey {FileKey}", fileKey);
                return Result.Failure<Stream>(StorageErrors.FailedToGetStream);
            }
            
            return Result.Success(response.ResponseStream);
        }
        catch (AmazonS3Exception s3Exception)
        {
            logger.LogError(s3Exception, "S3 error generating download URL for fileKey {FileKey}", fileKey);
            return Result.Failure<Stream>(StorageErrors.FailedToGetStream);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error generating download URL for fileKey {FileKey}", fileKey);
            return Result.Failure<Stream>(StorageErrors.UnexpectedError);
        }
    }
}

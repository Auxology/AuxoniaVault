using Main.Domain.Objects;
using Main.SharedKernel;

namespace Main.Application.Abstractions.Storage;

public interface IStorageServices
{
    Task<Result<StartS3Response>> StartMultiPartUploadAsync(string userId, string fileName, string contentType, CancellationToken cancellationToken);
    
    Task<Result<string>> GetPresignedUrlAsync(string userId, string fileKey, string uploadId, int partNumber, CancellationToken cancellationToken);
    
    Task<Result<string>> CompleteMultiPartUploadAsync(string userId, string fileKey, string uploadId, List<PartETag> partETags, CancellationToken cancellationToken);
    
    Task<Result> RemoveFileAsync(string fileKey, CancellationToken cancellationToken);
    
    Task<Result<string>> GetDownloadUrlAsync(string fileKey, CancellationToken cancellationToken);
    
    Task<Result<Stream>> GetFileStreamAsync(string fileKey, CancellationToken cancellationToken);
}
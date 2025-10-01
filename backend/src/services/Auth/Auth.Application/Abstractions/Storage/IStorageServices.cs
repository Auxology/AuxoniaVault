using Microsoft.AspNetCore.Http;

namespace Auth.Application.Abstractions.Storage;

public interface IStorageServices
{
    Task<string> PutObjectAsync(IFormFile file, CancellationToken cancellationToken);
}
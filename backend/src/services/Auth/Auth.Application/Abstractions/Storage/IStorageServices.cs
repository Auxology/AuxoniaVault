using Auth.SharedKernel;
using Microsoft.AspNetCore.Http;

namespace Auth.Application.Abstractions.Storage;

public interface IStorageServices
{
    Task<Result<string>> PutObjectAsync(IFormFile file, CancellationToken cancellationToken);
}
using Gateway.SharedKernel;

namespace Gateway.Application.Abstractions.Authentication;

public interface ITokenValidator
{
    Task<Result<bool>> IsValidAsync(string token, CancellationToken cancellationToken = default);
}
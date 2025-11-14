using Gateway.SharedKernel;

namespace Gateway.Application.Abstractions.Authentication;

public interface ITokenStorage
{
    Task StoreAccessTokenAsync(string refreshToken, string accessToken, CancellationToken cancellationToken = default);
    
    Task<Result<string>> GetAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    
    Task RemoveAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
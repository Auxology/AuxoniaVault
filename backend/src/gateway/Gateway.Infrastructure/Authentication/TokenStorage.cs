using Gateway.Application.Abstractions.Authentication;
using Gateway.Infrastructure.Settings;
using Gateway.SharedKernel;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Gateway.Infrastructure.Authentication;

internal sealed class TokenStorage(IDistributedCache distributedCache, IOptions<JwtSettings> jwtSettings) : ITokenStorage
{
    public async Task StoreAccessTokenAsync(string refreshToken, string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(jwtSettings.Value.ExpirationInMinutes - 1),
        };

        await distributedCache.SetStringAsync(refreshToken, accessToken, options, cancellationToken);
    }

    public async Task<Result<string>> GetAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Failure<string>(Error.NullValue);
        
        string? accessToken = await distributedCache.GetStringAsync(refreshToken, cancellationToken);
        
        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<string>(Error.NullValue);
        
        return Result.Success(accessToken);
    }

    public async Task RemoveAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;
        
        await distributedCache.RemoveAsync(refreshToken, cancellationToken);
    }
}
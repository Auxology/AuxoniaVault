using Microsoft.Extensions.Caching.Distributed;
using Shared.Abstractions.Authentication;
using Shared.Abstractions.Constants;

namespace Billing.Infrastructure.Authentication;

internal sealed class SessionBlacklistCache(IDistributedCache distributedCache) : ISessionBlacklistCache
{
    public async Task BlacklistSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        string key = GetCacheKey(sessionId);
       
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SessionsBlockListConstants.BlackListDurationInMinutes)
        };
        
        await distributedCache.SetStringAsync(key, "1", options, cancellationToken);
    }

    public async Task<bool> IsSessionBlacklistedAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        string key = GetCacheKey(sessionId);
        
        string? value = await distributedCache.GetStringAsync(key, cancellationToken);
        
        return value is not null;
    }
    
    private static string GetCacheKey(Guid sessionId) => $"{SessionsBlockListConstants.BlackListPrefix}{sessionId}";
}
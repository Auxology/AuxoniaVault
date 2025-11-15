namespace Shared.Abstractions.Authentication;

public interface ISessionBlacklistCache
{
    Task BlacklistSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    
    Task<bool> IsSessionBlacklistedAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
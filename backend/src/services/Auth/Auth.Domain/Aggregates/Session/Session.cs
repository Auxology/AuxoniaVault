using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;

namespace Auth.Domain.Aggregates.Session;

public class Session : Entity, IAggregateRoot
{
    public SessionId Id { get; private set; }
    
    public UserId UserId { get; private set; }
    
    public string Token { get; private set; }
    
    public string IpAddress { get; private set; }
    
    public string UserAgent { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    private Session() {} // For EF Core

    private Session(UserId userId, string token, string ipAddress, string userAgent, IDateTimeProvider dateTimeProvider)
    {
        Id = SessionId.New();
        UserId = userId;
        Token = token;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = dateTimeProvider.UtcNow;
        ExpiresAt = CreatedAt.AddDays(SessionConstants.ExpiresInDays);
    }

    public static Result<Session> Create(UserId userId, string token, string ipAddress, string userAgent,
        IDateTimeProvider dateTimeProvider)
    {
        if (userId.IsEmpty())
            return Result.Failure<Session>(SessionErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure<Session>(SessionErrors.TokenRequired);
        
        if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(userAgent))
            return Result.Failure<Session>(SessionErrors.InfoRequired);
        
        var session = new Session(userId, token, ipAddress, userAgent, dateTimeProvider);
        
        return Result.Success(session);
    }
}
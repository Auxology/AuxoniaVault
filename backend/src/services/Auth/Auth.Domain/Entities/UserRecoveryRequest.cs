using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;

namespace Auth.Domain.Entities;

public class UserRecoveryRequest : Entity
{
    public int Id { get; private set; }
    
    public string UniqueIdentifier { get; private set; } 
    
    public UserId UserId { get; private set; }
    
    public DateTimeOffset ApprovedAt { get; private set; }
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    public bool IsCompleted { get; private set; }
    
    public string IpAddress { get; private set; }
    
    public string UserAgent { get; private set; }
    
    public string? NewEmail { get; private set; } 
    
    public DateTimeOffset? CompletedAt { get; private set; }
    
    private UserRecoveryRequest() { } // For EF Core

    private UserRecoveryRequest
    (
        string uniqueIdentifier,
        UserId userId,
        string ipAddress,
        string userAgent,
        DateTimeOffset utcNow
    )
    {
        UserId = userId;
        UniqueIdentifier = uniqueIdentifier;
        ApprovedAt = utcNow;
        ExpiresAt = utcNow.AddMinutes(UserRecoveryRequestConstants.ExpiresInMinutes);
        IsCompleted = false;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public static Result<UserRecoveryRequest> Create
    (
        string uniqueIdentifier,
        UserId userId,
        string ipAddress,
        string userAgent,
        IDateTimeProvider dateTimeProvider
    )
    {
        if (userId.IsEmpty())
            return Result.Failure<UserRecoveryRequest>(UserRecoveryRequestErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(uniqueIdentifier))
            return Result.Failure<UserRecoveryRequest>(UserRecoveryRequestErrors.UniqueIdentifierRequired);
        
        if (string.IsNullOrWhiteSpace(ipAddress))
            return Result.Failure<UserRecoveryRequest>(UserRecoveryRequestErrors.IpAddressRequired);
        
        if (string.IsNullOrWhiteSpace(userAgent))
            return Result.Failure<UserRecoveryRequest>(UserRecoveryRequestErrors.UserAgentRequired);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        var request = new UserRecoveryRequest(
            uniqueIdentifier,
            userId,
            ipAddress,
            userAgent,
            utcNow
        );
        
        return Result.Success(request);
    }
    
    internal Result Complete(string newEmail, IDateTimeProvider dateTimeProvider)
    {
        if (IsCompleted)
            return Result.Failure(UserRecoveryRequestErrors.AlreadyCompleted);
        
        if (ExpiresAt <= dateTimeProvider.UtcNow)
            return Result.Failure(UserRecoveryRequestErrors.Expired);
        
        IsCompleted = true;
        NewEmail = newEmail;
        CompletedAt = dateTimeProvider.UtcNow;
        
        return Result.Success();
    }
}
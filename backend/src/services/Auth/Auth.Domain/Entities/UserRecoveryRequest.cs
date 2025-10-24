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
    
    public string? NewEmail { get; private set; } 
    
    public DateTimeOffset? CompletedAt { get; private set; }
    
    private UserRecoveryRequest() { } // For EF Core

    private UserRecoveryRequest
    (
        string uniqueIdentifier,
        UserId userId,
        DateTimeOffset utcNow
    )
    {
        UserId = userId;
        UniqueIdentifier = uniqueIdentifier;
        ApprovedAt = utcNow;
        ExpiresAt = utcNow.AddMinutes(UserRecoveryRequestConstants.ExpiresInMinutes);
        IsCompleted = false;
    }

    public static Result<UserRecoveryRequest> Create
    (
        string uniqueIdentifier,
        UserId userId,
        IDateTimeProvider dateTimeProvider
    )
    {
        if (userId.IsEmpty())
            return Result.Failure<UserRecoveryRequest>(UserRecoveryRequestErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(uniqueIdentifier))
            return Result.Failure<UserRecoveryRequest>(UserRecoveryRequestErrors.UniqueIdentifierRequired);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        var request = new UserRecoveryRequest(
            uniqueIdentifier,
            userId,
            utcNow
        );
        
        return Result.Success(request);
    }
    
    public Result Complete(string newEmail, IDateTimeProvider dateTimeProvider)
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
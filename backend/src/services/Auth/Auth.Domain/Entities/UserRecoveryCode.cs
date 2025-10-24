using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;

namespace Auth.Domain.Entities;

public class UserRecoveryCode : Entity
{
    public int Id { get; private set; }
    
    public UserId UserId { get; private set; }
    
    public string HashedCode { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? LastUsedAt { get; private set; }
    
    private UserRecoveryCode() { } // For EF Core

    private UserRecoveryCode(UserId userId, string hashedCode, DateTimeOffset utcNow)
    {
        UserId = userId;
        HashedCode = hashedCode;
        CreatedAt = utcNow;
    }

    public static Result<UserRecoveryCode> Create(UserId userId, string hashedCode, IDateTimeProvider dateTimeProvider)
    {
        if (userId.IsEmpty())
            return Result.Failure<UserRecoveryCode>(UserRecoveryCodeErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(hashedCode))
            return Result.Failure<UserRecoveryCode>(UserRecoveryCodeErrors.HashedCodeRequired);
        
        var utcNow = dateTimeProvider.UtcNow;
        
        var userRecoveryCode = new UserRecoveryCode(userId, hashedCode, utcNow);
        
        return Result.Success(userRecoveryCode);
    }
}
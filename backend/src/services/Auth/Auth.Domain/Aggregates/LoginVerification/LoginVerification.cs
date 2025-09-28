using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.Events;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;

namespace Auth.Domain.Aggregates.LoginVerification;

public class LoginVerification : Entity, IAggregateRoot
{
    public LoginVerificationId Id { get; private set; }
    
    public EmailAddress Identifier { get; private set; }
    
    public int Value { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset UpdatedAt { get; private set; }
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    private LoginVerification() {} // For EF Core

    private LoginVerification(EmailAddress identifier, int value, DateTimeOffset utcNow)
    {
        Id = LoginVerificationId.New();
        Identifier = identifier;
        Value = value;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
        ExpiresAt = CreatedAt.AddMinutes(LoginVerificationConstants.ExpiresInMinutes);
    }
    
    public static Result<LoginVerification> Create(EmailAddress identifier, int value, DateTimeOffset utcNow)
    {
        if (value < 100000 || value > 999999)
            return Result.Failure<LoginVerification>(LoginVerificationErrors.InvalidValue);
        
        var loginVerification = new LoginVerification(identifier, value, utcNow);
        
        loginVerification.Raise(new LoginRequestedDomainEvent
        (
            Email: loginVerification.Identifier.Value,
            Token: loginVerification.Value,
            RequestedAt: loginVerification.CreatedAt
        ));
        
        return Result.Success(loginVerification);
    }
}
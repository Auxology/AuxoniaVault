using Auth.Domain.Constants;
using Auth.Domain.Entities;
using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;

namespace Auth.Domain.Aggregates.User;

public class User : Entity, IAggregateRoot
{
    public UserId Id { get; private set; }
    
    public string Name { get; private set; }
    
    public EmailAddress Email { get; private set; }
    
    public string? Avatar { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    public virtual ICollection<EmailChangeRequest> EmailChangeRequests { get; private set; }
    
    private User() { } // For EF Core
    
    private User(string name, EmailAddress email, DateTimeOffset utcNow)
    {
        Id = UserId.New();
        Name = name;
        Email = email;
        CreatedAt = utcNow;
        Avatar = null;
        UpdatedAt = null;
        EmailChangeRequests = [];
    }

    public static Result<User> Create(string name, EmailAddress email, IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>(UserErrors.NameRequired);
        
        if (name.Length > UserConstants.MaxNameLength)
            return Result.Failure<User>(UserErrors.NameTooLong);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        var user = new User(name, email, utcNow);
        
        return Result.Success(user);
    }
    
    public Result ChangeName(string newName, IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(UserErrors.NameRequired);
        
        if (newName.Length > UserConstants.MaxNameLength)
            return Result.Failure(UserErrors.NameTooLong);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        Name = newName;
        UpdatedAt = utcNow;
        
        return Result.Success();
    }

    public Result<EmailChangeRequest> RequestEmailChange(EmailAddress newEmail, IDateTimeProvider dateTimeProvider)
    {
        var activeRequest = EmailChangeRequests
            .FirstOrDefault(r => r.CurrentStep != EmailChangeStep.Completed && r.ExpiresAt > dateTimeProvider.UtcNow);
        
        if (activeRequest != null)
            return Result.Failure<EmailChangeRequest>(EmailChangeRequestErrors.ActiveRequestExists);
        
        var emailChangeRequestResult = EmailChangeRequest.StartTraditional(Id, Email, newEmail, dateTimeProvider);
        
        if (emailChangeRequestResult.IsFailure)
            return Result.Failure<EmailChangeRequest>(emailChangeRequestResult.Error);
        
        EmailChangeRequests.Add(emailChangeRequestResult.Value);

        return emailChangeRequestResult;        
    }
}

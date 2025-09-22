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
    
    private User() { } // For EF Core
    
    private User(string name, EmailAddress email, DateTimeOffset utcNow)
    {
        Id = UserId.New();
        Name = name;
        Email = email;
        CreatedAt = utcNow;
        Avatar = null;
        UpdatedAt = null;
    }
}

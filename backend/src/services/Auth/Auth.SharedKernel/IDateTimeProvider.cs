namespace Auth.SharedKernel;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
    
    DateTimeOffset Now { get; }
}
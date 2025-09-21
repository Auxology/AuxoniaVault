namespace Auth.Application.Abstractions.Database;

public interface IAuthDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
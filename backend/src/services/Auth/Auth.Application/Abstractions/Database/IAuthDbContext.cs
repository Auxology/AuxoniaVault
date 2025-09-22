using Auth.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Abstractions.Database;

public interface IAuthDbContext
{
    DbSet<User> Users { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
using Auth.Domain.Aggregates.LoginVerification;
using Auth.Domain.Aggregates.Session;
using Auth.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Abstractions.Database;

public interface IAuthDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<LoginVerification> LoginVerifications { get; }
    
    DbSet<Session> Sessions { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
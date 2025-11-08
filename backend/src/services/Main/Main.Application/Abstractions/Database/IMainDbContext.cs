using Main.Domain.Aggregates.Account;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Abstractions.Database;

public interface IMainDbContext
{
    public DbSet<Account> Accounts { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
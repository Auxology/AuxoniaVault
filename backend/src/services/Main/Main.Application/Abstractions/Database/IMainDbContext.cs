using Main.Domain.Aggregates.Account;
using Main.Domain.Aggregates.FileMetadata;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Abstractions.Database;

public interface IMainDbContext
{
    DbSet<Account> Accounts { get; }
    
    DbSet<FileMetadata> Files { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
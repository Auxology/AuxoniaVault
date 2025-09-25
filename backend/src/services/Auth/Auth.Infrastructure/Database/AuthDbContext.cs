using Auth.Application.Abstractions.Database;
using Auth.Domain.Aggregates.LoginVerification;
using Auth.Domain.Aggregates.Session;
using Auth.Domain.Aggregates.User;
using Auth.Infrastructure.DomainEvents;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Database;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options, IDomainEventDispatcher dispatcher)
    : DbContext(options), IAuthDbContext
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<LoginVerification> LoginVerifications { get; set; }
    
    public DbSet<Session> Sessions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);
         
        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);
        
        await PublishDomainEvents();
        
        return result;
    }

    private async Task PublishDomainEvents()
    {
        List<IDomainEvent> domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> events = entity.DomainEvents;
                
                entity.ClearDomainEvents();
                
                return events;
            })
            .ToList();
        
        await dispatcher.DispatchAsync(domainEvents);
    }
}
using Main.Application.Abstractions.Database;
using Main.Domain.Aggregates.Account;
using Main.Infrastructure.DomainEvents;
using Main.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Main.Infrastructure.Database;

public sealed class MainDbContext(DbContextOptions<MainDbContext> options, IDomainEventDispatcher dispatcher)
    : DbContext(options), IMainDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Account> Accounts { get; set; }

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
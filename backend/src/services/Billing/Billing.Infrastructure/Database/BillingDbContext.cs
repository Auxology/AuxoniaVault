using Billing.Infrastructure.Database;
using Billing.Application.Abstractions.Database;
using Billing.Infrastructure.DomainEvents;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Database;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options, IDomainEventDispatcher dispatcher)
    : DbContext(options), IBillingDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
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
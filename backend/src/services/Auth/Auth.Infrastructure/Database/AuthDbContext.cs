using Auth.Application.Abstractions.Database;
using Auth.Domain.Aggregates.AuditLog;
using Auth.Domain.Aggregates.LoginVerification;
using Auth.Domain.Aggregates.Session;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Entities;
using Auth.Infrastructure.AuditLogs;
using Auth.Infrastructure.DomainEvents;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Database;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options, IDomainEventDispatcher dispatcher, IDateTimeProvider dateTimeProvider)
    : DbContext(options), IAuthDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<LoginVerification> LoginVerifications { get; set; }

    public DbSet<Session> Sessions { get; set; }

    public DbSet<EmailChangeRequest> EmailChangeRequests { get; set; }
    
    public DbSet<UserRecoveryRequest> UserRecoveryRequests { get; set; }
    
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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
        
        int result = await base.SaveChangesAsync(cancellationToken);
        
        List<AuditLog> auditLogs = domainEvents
            .Where(@event => @event is IAuditLoggedDomainEvent)
            .Select(@event => AuditLogFactory.CreateFromDomainEvent(@event, dateTimeProvider))
            .ToList();
        
        if (auditLogs.Any())
        {
            await AuditLogs.AddRangeAsync(auditLogs, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }

        await dispatcher.DispatchAsync(domainEvents, cancellationToken);
        
        return result;
    }
}
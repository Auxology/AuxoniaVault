using Billing.Domain.Aggregate.Customer;
using Billing.Domain.Aggregate.Webhook;
using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Abstractions.Database;

public interface IBillingDbContext
{
    DbSet<Customer> Customers { get; }
    
    DbSet<Subscription> Subscriptions { get; }
    
    DbSet<WebhookEvent> WebhookEvents { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
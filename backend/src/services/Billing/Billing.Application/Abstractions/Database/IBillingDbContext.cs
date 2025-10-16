using Billing.Domain.Aggregate.Customer;
using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Abstractions.Database;

public interface IBillingDbContext
{
    DbSet<Customer> Customers { get; }
    
    DbSet<Subscription> Subscriptions { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
using Billing.Domain.Aggregate.Customer;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Abstractions.Database;

public interface IBillingDbContext
{
    DbSet<Customer> Customers { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
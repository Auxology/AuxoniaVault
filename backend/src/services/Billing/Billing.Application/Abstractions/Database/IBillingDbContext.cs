namespace Billing.Application.Abstractions.Database;

public interface IBillingDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
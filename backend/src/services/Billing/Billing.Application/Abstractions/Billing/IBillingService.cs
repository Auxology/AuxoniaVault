using Billing.SharedKernel;

namespace Billing.Application.Abstractions.Billing;

public interface IBillingService
{
    Task GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    
    Task<Result<bool>> ValidateUserExistsAsync(Guid userId, CancellationToken cancellationToken);
}
using Billing.Application.Abstractions.Authentication;
using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Abstractions.Services;
using Billing.Application.Errors;
using Billing.Domain.Aggregate.Customer;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Billing.CreateBillingPortalSession;

internal sealed class CreateBillingPortalSessionCommandHandler(
    IBillingDbContext context,
    IUserContext userContext,
    IStripeBillingPortalService stripeBillingPortalService
)
    : ICommandHandler<CreateBillingPortalSessionCommand, string>
{
    public async Task<Result<string>> Handle(CreateBillingPortalSessionCommand request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);
        
        Customer? customer = await context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        
        if (customer is null)
            return Result.Failure<string>(CustomerErrors.CustomerNotFound);

        Result<string> portalSessionResult =
            await stripeBillingPortalService.CreateBillingPortalSessionAsync(customer.StripeCustomerId,
                cancellationToken);
        
        if (portalSessionResult.IsFailure)
            return Result.Failure<string>(portalSessionResult.Error);
        
        return Result.Success(portalSessionResult.Value);
    }
}
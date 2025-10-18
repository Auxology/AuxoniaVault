using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Errors;
using Billing.Domain.Aggregate.Customer;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Webhooks.ProcessSubscriptionDeleted;

internal sealed class ProcessSubscriptionDeletedCommandHandler(IBillingDbContext context, IDateTimeProvider dateTimeProvider) : ICommandHandler<ProcessSubscriptionDeletedCommand>
{
    public async Task<Result> Handle(ProcessSubscriptionDeletedCommand request, CancellationToken cancellationToken)
    {
        Customer? customer = await context.Customers
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.StripeCustomerId == request.StripeCustomerId, cancellationToken);
        
        if (customer is null)
            return Result.Failure(CustomerErrors.CustomerNotFound);

        Result cancelResult = customer.CompleteSubscriptionCancellation
        (
            request.StripeSubscriptionId,
            request.ProductName,
            request.PriceFormatted,
            request.CurrentPeriodEnd,
            request.CurrentPeriodStart,
            dateTimeProvider
        );
        
        if (cancelResult.IsFailure)
            return Result.Failure(cancelResult.Error);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}

using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Errors;
using Billing.Domain.Aggregate.Customer;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Webhooks.ProcessSubscriptionUpdated;

internal sealed class ProcessSubscriptionUpdatedCommandHandler(IBillingDbContext context, IDateTimeProvider dateTimeProvider) : ICommandHandler<ProcessSubscriptionUpdatedCommand>
{
    public async Task<Result> Handle(ProcessSubscriptionUpdatedCommand request, CancellationToken cancellationToken)
    {
        Customer? customer = await context.Customers
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.StripeCustomerId == request.StripeCustomerId, cancellationToken);
        
        if (customer is null)
            return Result.Failure(CustomerErrors.CustomerNotFound);

        Result updateResult;

        if (request.CancelAtPeriodEnd && request.Status == SubscriptionStatus.Active)
        {
            updateResult = customer.CancelSubscriptionAtPeriodEnd
            (
                request.StripeSubscriptionId,
                request.CurrentPeriodStart,
                request.CurrentPeriodEnd,
                dateTimeProvider
            );
        }
        else
        {
            updateResult = customer.UpdateSubscription
            (
                request.StripeSubscriptionId,
                request.Status,
                request.CurrentPeriodStart,
                request.CurrentPeriodEnd,
                request.CancelAtPeriodEnd,
                dateTimeProvider
            );
        }
        
        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
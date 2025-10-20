using Billing.Application.Abstractions.Authentication;
using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Errors;
using Billing.Domain.Aggregate.Customer;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Billing.GetUsersSubscriptionHistories;

internal sealed class GetUsersSubscriptionHistoriesQueryHandler(IBillingDbContext context, IUserContext userContext)
    : IQueryHandler<GetUsersSubscriptionHistoriesQuery, List<SubscriptionHistoryReadModel>>
{
    public async Task<Result<List<SubscriptionHistoryReadModel>>> Handle(GetUsersSubscriptionHistoriesQuery request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);
        
        Customer? customer = await context.Customers
            .Include(c => c.SubscriptionHistories)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer is null)
            return Result.Failure<List<SubscriptionHistoryReadModel>>(CustomerErrors.CustomerNotFound);
        
        if (!customer.SubscriptionHistories.Any())
            return Result.Success(new List<SubscriptionHistoryReadModel>());

        List<SubscriptionHistoryReadModel> subscriptionHistories = customer.SubscriptionHistories
            .Select(sh => new SubscriptionHistoryReadModel
            (
                sh.Id,
                sh.StripeSubscriptionId,
                sh.ProductName,
                sh.PriceFormatted,
                sh.EventType,
                sh.PeriodStart,
                sh.PeriodEnd
            ))
            .ToList();
        
        return Result.Success(subscriptionHistories);
    }
}
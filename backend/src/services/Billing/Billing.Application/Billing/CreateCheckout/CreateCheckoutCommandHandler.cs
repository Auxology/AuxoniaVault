using Billing.Application.Abstractions.Authentication;
using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Abstractions.Services;
using Billing.Application.Errors;
using Billing.Domain.Aggregate.Customer;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Billing.CreateCheckout;

internal sealed class CreateCheckoutCommandHandler(
    IBillingDbContext context,
    IUserContext userContext,
    IStripeCheckoutService stripeCheckoutService,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateCheckoutCommand, string>
{
    public async Task<Result<string>> Handle(CreateCheckoutCommand request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);

        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer is null)
            return Result.Failure<string>(CustomerErrors.CustomerNotInitialized);

        var activeSubscription = await context.Subscriptions
            .FirstOrDefaultAsync(
                s => s.StripeCustomerId == customer.StripeCustomerId && s.Status == SubscriptionStatus.Active,
                cancellationToken);

        if (activeSubscription is not null && activeSubscription.CurrentPeriodEnd > dateTimeProvider.UtcNow)
            return Result.Failure<string>(SubscriptionErrors.ActiveSubscriptionAlreadyExists);

        var checkoutSessionResult = await stripeCheckoutService.CreateCheckoutSessionAsync
        (
            customer.StripeCustomerId,
            request.PriceId,
            cancellationToken
        );

        if (checkoutSessionResult.IsFailure)
            return Result.Failure<string>(checkoutSessionResult.Error);

        return Result.Success(checkoutSessionResult.Value);
    }
}
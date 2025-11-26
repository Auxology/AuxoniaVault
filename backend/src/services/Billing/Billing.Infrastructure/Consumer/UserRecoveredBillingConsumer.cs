using Billing.Application.Abstractions.Database;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Stripe;

namespace Billing.Infrastructure.Consumer;

public sealed class UserRecoveredBillingConsumer
(
    IBillingDbContext dbContext,
    CustomerService customerService,
    ILogger<UserRecoveredBillingConsumer> logger
) : IConsumer<UserRecoveredContract>
{
    public async Task Consume(ConsumeContext<UserRecoveredContract> context)
    {
        logger.LogInformation("Received UserRecoveredContract for User ID: {UserId}", context.Message.UserId);

        var message = context.Message;
        
        UserId userId = UserId.UnsafeFromGuid(message.UserId);
        
        var existingCustomer = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId, context.CancellationToken);

        if (existingCustomer is null)
        {
            logger.LogWarning("No existing customer found for User ID: {UserId}. Cannot recover user.", message.UserId);
            return;
        }
        
        var updateOptions = new CustomerUpdateOptions
        {
            Metadata = new Dictionary<string, string>
            {
                {"userId", message.UserId.ToString()},
                {"account_recovered", "true"}
            }
        };
        
        await customerService.UpdateAsync(existingCustomer.StripeCustomerId, updateOptions);
        
        Result updateResult = existingCustomer.UpdateEmail(message.NewEmail);
        
        if (updateResult.IsFailure)
        {
            logger.LogError("Failed to update recovery status for Customer ID: {CustomerId}. Errors: {Errors}", existingCustomer.StripeCustomerId, string.Join(", ", updateResult.Error));
            return;
        }
        
        await dbContext.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Recovered user for Customer ID: {CustomerId}", existingCustomer.StripeCustomerId);
    }
}
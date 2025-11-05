using Billing.Application.Abstractions.Database;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Stripe;

namespace Billing.Infrastructure.Consumer;

public sealed class UserNameChangedBillingConsumer(
    IBillingDbContext dbContext,
    CustomerService customerService,
    ILogger<UserNameChangedBillingConsumer> logger) : IConsumer<UserNameChangedContract>
{
    public async Task Consume(ConsumeContext<UserNameChangedContract> context)
    {
        logger.LogInformation("Received UserNameChangedContract for User ID: {UserId}, New Name: {NewName}", context.Message.UserId, context.Message.NewName);

        var message = context.Message;

        var updateOptions = new CustomerUpdateOptions
        {
            Name = message.NewName,
            Metadata = new Dictionary<string, string>
            {
                {"userId", message.UserId.ToString()},
            }
        };

        UserId userId = UserId.UnsafeFromGuid(message.UserId);
        
        var existing = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (existing is null)
        {
            logger.LogWarning("No existing customer found for User ID: {UserId}. Cannot update name.", message.UserId);
            return;
        }
        
        var customer = await customerService.UpdateAsync(existing.StripeCustomerId, updateOptions);
        
        Result updateResult = existing.UpdateName(customer.Name);
        
        if (updateResult.IsFailure)
        {
            logger.LogError("Failed to update name for Customer ID: {CustomerId}. Errors: {Errors}",
                existing.StripeCustomerId, string.Join(", ", updateResult.Error));
            return;
        }
        
        await dbContext.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Updated name for Customer ID: {CustomerId} to New Name: {NewName}",
            existing.StripeCustomerId, message.NewName);
    }
}
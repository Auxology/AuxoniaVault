using Billing.Application.Abstractions.Database;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Stripe;

namespace Billing.Infrastructure.Consumer;

public sealed class EmailChangedBillingConsumer(
    IBillingDbContext dbContext,
    CustomerService customerService,
    ILogger<UserCreatedBillingConsumer> logger) : IConsumer<EmailChangedContract>
{
    public async Task Consume(ConsumeContext<EmailChangedContract> context)
    {
        logger.LogInformation("Received EmailChangedContract for User ID: {UserId}, New Email: {NewEmail}", context.Message.UserId, context.Message.NewEmail);

        var message = context.Message;
        
        UserId userId = UserId.UnsafeFromGuid(message.UserId);
        
        var existingCustomer = await dbContext.Customers
            .FirstOrDefaultAsync(c=> c.UserId == userId, context.CancellationToken);

        if (existingCustomer is null)
        {
            logger.LogWarning("No existing customer found for User ID: {UserId}. Cannot update email.", message.UserId);
            return;
        }
        
        var updateOptions = new CustomerUpdateOptions
        {
            Email = message.NewEmail,
            Metadata = new Dictionary<string, string>
            {
                {"userId", message.UserId.ToString()},
            }
        };
        
        await customerService.UpdateAsync(existingCustomer.StripeCustomerId, updateOptions);
        
        Result updateResult = existingCustomer.UpdateEmail(message.NewEmail);
        
        if (updateResult.IsFailure)
        {
            logger.LogError("Failed to update email for Customer ID: {CustomerId}. Errors: {Errors}", existingCustomer.StripeCustomerId, string.Join(", ", updateResult.Error));
            return;
        }
        
        await dbContext.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Updated email for Customer ID: {CustomerId} to New Email: {NewEmail}", existingCustomer.StripeCustomerId, message.NewEmail);
    }
}
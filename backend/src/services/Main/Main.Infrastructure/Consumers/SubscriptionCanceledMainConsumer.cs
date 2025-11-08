using Main.Application.Abstractions.Database;
using Main.Domain.Aggregates.Account;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Main.Infrastructure.Consumers;

public sealed class SubscriptionCanceledMainConsumer(
    IMainDbContext dbContext,
    ILogger<SubscriptionCanceledMainConsumer> logger,
    IDateTimeProvider dateTimeProvider) : IConsumer<SubscriptionCanceledContract>
{
    public async Task Consume(ConsumeContext<SubscriptionCanceledContract> context)
    {
        logger.LogInformation("Received SubscriptionCanceledContract for User ID: {UserId}, Tier: {Tier}",
            context.Message.UserId, context.Message.Tier);
        
        SubscriptionCanceledContract message = context.Message;
        
        UserId typedUserId = UserId.UnsafeFromGuid(message.UserId);
        
        Account? existingAccount = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == typedUserId, context.CancellationToken);
        
        if (existingAccount is null)
        {
            logger.LogWarning("No account found for User ID: {UserId}. Cannot cancel subscription.",
                message.UserId);
            return;
        }

        Result changeTierResult = existingAccount.ChangeTier
        (
            message.Tier,
            dateTimeProvider
        );
        
        if (changeTierResult.IsFailure)
        {
            logger.LogError("Failed to change tier for User ID: {UserId}. Errors: {Errors}",
                message.UserId, string.Join(", ", changeTierResult.Error));
            return;
        }
        
        dbContext.Accounts.Update(existingAccount);
        
        await dbContext.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Canceled subscription for User ID: {UserId}, new Tier: {Tier}",
            message.UserId, message.Tier);
    }
}
using Main.Application.Abstractions.Database;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Main.Infrastructure.Consumers;

public sealed class CustomerTierInitializedMainConsumer
(
    IMainDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerTierInitializedMainConsumer> logger
) : IConsumer<CustomerTierInitializedContract>
{
    public async Task Consume(ConsumeContext<CustomerTierInitializedContract> context)
    {
        logger.LogInformation("Received CustomerTierInitializedContract for User ID: {UserId}, Tier: {Tier}",
            context.Message.UserId, context.Message.Tier);
        
        CustomerTierInitializedContract message = context.Message;
        
        UserId userId = UserId.UnsafeFromGuid(message.UserId);
        
        var existingAccount = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, context.CancellationToken);

        if (existingAccount is null)
        {
            logger.LogWarning("No account found for User ID: {UserId}. Cannot initialize customer tier.",
                message.UserId);
            return;
        }

        Result changeTierResult = existingAccount.ChangeTier
        (
            newTier: message.Tier,
            dateTimeProvider: dateTimeProvider
        );
        
        if (changeTierResult.IsFailure)
        {
            logger.LogError("Failed to change tier for User ID: {UserId}. Errors: {Errors}",
                message.UserId, string.Join(", ", changeTierResult.Error));
            return;
        }
        
        dbContext.Accounts.Update(existingAccount);
        
        await dbContext.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Updated tier for User ID: {UserId} to Tier: {Tier}",
            message.UserId, message.Tier);
    }
}
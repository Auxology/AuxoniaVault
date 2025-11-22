using Main.Application.Abstractions.Database;
using Main.Domain.Aggregates.Account;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Main.Infrastructure.Consumers.User;

public sealed class UserCreatedMainConsumer
(
    IMainDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<UserCreatedMainConsumer> logger
) : IConsumer<UserCreatedContract>
{
    public async Task Consume(ConsumeContext<UserCreatedContract> context)
    {
        logger.LogInformation("Received UserCreatedContract for User ID: {UserId}, Email: {Email}",
            context.Message.UserId, context.Message.Email);

        UserCreatedContract message = context.Message;
        
        UserId userId = UserId.UnsafeFromGuid(message.UserId);
        
        var existingAccount = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, context.CancellationToken);

        if (existingAccount is not null)
        {
            logger.LogWarning("Account already exists for User ID: {UserId}. Skipping creation.", message.UserId);
            return;
        }

        Result<Account> accountResult = Account.Create
        (
            id: userId,
            accountName: message.Name,
            accountEmail: message.Email,
            dateTimeProvider: dateTimeProvider
        );
        
        if (accountResult.IsFailure)
        {
            logger.LogError("Failed to create account for User ID: {UserId}. Errors: {Errors}",
                message.UserId, string.Join(", ", accountResult.Error));
            return;
        }
        
        await dbContext.Accounts.AddAsync(accountResult.Value, context.CancellationToken);
        
        await dbContext.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Created account for User ID: {UserId}, Email: {Email}",
            message.UserId, message.Email);
    }
}
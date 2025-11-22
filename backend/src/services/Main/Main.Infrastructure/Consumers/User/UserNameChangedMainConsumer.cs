using Main.Application.Abstractions.Database;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Main.Infrastructure.Consumers.User;

public sealed class UserNameChangedMainConsumer
(
    IMainDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<UserNameChangedMainConsumer> logger
) : IConsumer<UserNameChangedContract>
{
    public async Task Consume(ConsumeContext<UserNameChangedContract> context)
    {
        logger.LogInformation("Received UserNameChangedContract for User ID: {UserId}, New Name: {NewName}",
            context.Message.UserId, context.Message.NewName);

        UserNameChangedContract message = context.Message;

        UserId userId = UserId.UnsafeFromGuid(message.UserId);

        var existingAccount = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, context.CancellationToken);

        if (existingAccount is null)
        {
            logger.LogWarning("No account found for User ID: {UserId}. Cannot change name.", message.UserId);
            return;
        }
        
        Result nameChangeResult = existingAccount.ChangeName
        (
            newName: message.NewName,
            dateTimeProvider: dateTimeProvider
        );
        
        if (nameChangeResult.IsFailure)
        {
            logger.LogError("Failed to change name for User ID: {UserId}. Errors: {Errors}",
                message.UserId, string.Join(", ", nameChangeResult.Error));
            return;
        }
        
        dbContext.Accounts.Update(existingAccount);
        
        await dbContext.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Changed name for User ID: {UserId} to New Name: {NewName}",
            message.UserId, message.NewName);
    }
}
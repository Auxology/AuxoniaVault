using Main.Application.Abstractions.Database;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Main.Infrastructure.Consumers.User;

public sealed class UserRecoveredMainConsumer
(
    IMainDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<UserRecoveredMainConsumer> logger
) : IConsumer<UserRecoveredContract>
{
    public async Task Consume(ConsumeContext<UserRecoveredContract> context)
    {
        logger.LogInformation("Received UserRecoveredContract for User ID: {UserId}, Email: {Email}",
            context.Message.UserId, context.Message.NewEmail);
        
        UserRecoveredContract contract = context.Message;
        
        UserId userId = UserId.UnsafeFromGuid(contract.UserId);
        
        var existingAccount = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, context.CancellationToken);

        if (existingAccount is null)
        {
            logger.LogWarning("Account does not exist for User ID: {UserId}. Skipping recovery.", contract.UserId);
            return;
        }    
        
        Result changeEmailResult = existingAccount.ChangeEmail
        (
            newEmail: contract.NewEmail,
            dateTimeProvider: dateTimeProvider
        );
        
        if (changeEmailResult.IsFailure)
        {
            logger.LogError("Failed to change email for User ID: {UserId}. Errors: {Errors}",
                contract.UserId, string.Join(", ", changeEmailResult.Error));
            return;
        }
        
        dbContext.Accounts.Update(existingAccount);
        
        await dbContext.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Updated email for User ID: {UserId} to New Email: {NewEmail}",
            contract.UserId, contract.NewEmail);
    }
}
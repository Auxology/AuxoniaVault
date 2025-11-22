using Main.Application.Abstractions.Database;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Main.Infrastructure.Consumers.User;

public sealed class EmailChangedMainConsumer
(
    IMainDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<EmailChangedMainConsumer> logger
)
: IConsumer<EmailChangedContract>
{
    public async Task Consume(ConsumeContext<EmailChangedContract> context)
    {
        logger.LogInformation("Received EmailChangedContract for User ID: {UserId}, New Email: {NewEmail}",
            context.Message.UserId, context.Message.NewEmail);

        EmailChangedContract message = context.Message;

        UserId userId = UserId.UnsafeFromGuid(message.UserId);
        
        var existingAccount = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, context.CancellationToken);

        if (existingAccount is null)
        {
            logger.LogWarning("No account found for User ID: {UserId}. Cannot change email.", message.UserId);
            return;
        }
        
        Result emailChangeResult = existingAccount.ChangeEmail
        (
            newEmail: message.NewEmail,
            dateTimeProvider: dateTimeProvider
        );
        
        if (emailChangeResult.IsFailure)
        {
            logger.LogError("Failed to change email for User ID: {UserId}. Errors: {Errors}",
                message.UserId, string.Join(", ", emailChangeResult.Error));
            return;
        }
        
        dbContext.Accounts.Update(existingAccount);
        
        await dbContext.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Processed EmailChangedContract for User ID: {UserId}, New Email: {NewEmail}",
            message.UserId, message.NewEmail);
    }
}
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.CompleteRecovery;

internal sealed class CompleteRecoveryCommandHandler(IAuthDbContext context, IDateTimeProvider dateTimeProvider) : ICommandHandler<CompleteRecoveryCommand>
{
    public async Task<Result> Handle(CompleteRecoveryCommand request, CancellationToken cancellationToken)
    {
        var newEmailResult = EmailAddress.Create(request.NewEmail);
        
        if (newEmailResult.IsFailure)
            return Result.Failure(newEmailResult.Error);
        
        if (await context.Users.AnyAsync(u => u.Email == newEmailResult.Value, cancellationToken))
            return Result.Failure(UserErrors.EmailNotUnique);
        
        var recoveryRequest = await context.UserRecoveryRequests
            .SingleOrDefaultAsync(r => r.UniqueIdentifier == request.UniqueIdentifier, cancellationToken);
        
        if (recoveryRequest is null)
            return Result.Failure(UserErrors.InvalidRecoveryCode);
        
        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == recoveryRequest.UserId, cancellationToken);
        
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var completeResult = user.CompleteRecovery(recoveryRequest, newEmailResult.Value, dateTimeProvider);
        
        if (completeResult.IsFailure)
            return Result.Failure(completeResult.Error);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
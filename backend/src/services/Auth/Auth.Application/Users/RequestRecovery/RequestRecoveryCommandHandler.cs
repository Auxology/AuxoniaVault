using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Abstractions.Services;
using Auth.Application.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.RequestRecovery;

internal sealed class RequestRecoveryCommandHandler(IAuthDbContext context, ISecretHasher hasher, IGenerator generator, IDateTimeProvider dateTimeProvider) : ICommandHandler<RequestRecoveryCommand, string>
{
    public async Task<Result<string>> Handle(RequestRecoveryCommand request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(request.UserId);

        var user = await context.Users
            .Include(u => u.RecoveryCodes)
            .Include(u => u.RecoveryRequests)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure<string>(UserErrors.UserNotFound);
        
        List<string> recoveryCodes = user.RecoveryCodes
            .Select(rc => rc.HashedCode)
            .ToList();
        
        var isCorrect = await hasher.OneToManyVerifyAsync(recoveryCodes, request.RecoveryCode, cancellationToken);

        if (!isCorrect)
            return Result.Failure<string>(UserErrors.InvalidRecoveryCode);
        
        string uniqueIdentifier = await generator.GenerateUniqueIdentifier();
        
        var requestResult = user.ApproveRecoveryRequest(uniqueIdentifier, dateTimeProvider);
        
        if (requestResult.IsFailure)
            return Result.Failure<string>(requestResult.Error);
        
        await context.UserRecoveryRequests.AddAsync(requestResult.Value, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success(uniqueIdentifier);
    }
}
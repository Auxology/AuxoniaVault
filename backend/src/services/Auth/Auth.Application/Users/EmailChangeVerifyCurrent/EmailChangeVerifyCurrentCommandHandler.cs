using System.Security.Cryptography;
using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.User;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.EmailChangeVerifyCurrent;

internal sealed class EmailChangeVerifyCurrentCommandHandler(IAuthDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<EmailChangeVerifyCurrentCommand>
{
    public async Task<Result> Handle(EmailChangeVerifyCurrentCommand request, CancellationToken cancellationToken)
    {
        Guid currentUserId = userContext.UserId;

        UserId userId = UserId.UnsafeFromGuid(currentUserId);

        var user = await context.Users
            .Include(u => u.EmailChangeRequests)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        int newEmailOtp = RandomNumberGenerator.GetInt32(100000, 999999);

        Result verifyResult = user.VerifyCurrentEmail(request.CurrentOtp, newEmailOtp, dateTimeProvider);

        if (verifyResult.IsFailure)
            return Result.Failure(verifyResult.Error);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
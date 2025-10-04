using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.User;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.EmailChangeVerifyNew;

internal sealed class EmailChangeVerifyNewCommandHandler(IAuthDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<EmailChangeVerifyNewCommand>
{
    public async Task<Result> Handle(EmailChangeVerifyNewCommand request, CancellationToken cancellationToken)
    {
        Guid currentUserId = userContext.UserId;

        UserId userId = UserId.UnsafeFromGuid(currentUserId);

        var user = await context.Users
            .Include(u => u.EmailChangeRequests)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        Result verifyResult = user.VerifyNewEmail(request.NewOtp, dateTimeProvider);

        if (verifyResult.IsFailure)
            return Result.Failure(verifyResult.Error);

        var userSessions = await context.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);

        context.Sessions.RemoveRange(userSessions);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
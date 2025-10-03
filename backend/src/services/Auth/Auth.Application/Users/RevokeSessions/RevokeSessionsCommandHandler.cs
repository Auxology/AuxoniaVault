using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.Session;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.RevokeSessions;

internal sealed class RevokeSessionsCommandHandler(IAuthDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RevokeSessionsCommand>
{
    public async Task<Result> Handle(RevokeSessionsCommand request, CancellationToken cancellationToken)
    {
        Session? currentSession = await context.Sessions
            .FirstOrDefaultAsync(s => s.Token == request.RefreshToken, cancellationToken);

        if (currentSession is null)
            return Result.Failure(SessionErrors.SessionNotFound);

        if (!currentSession.IsActive(dateTimeProvider) || currentSession.ExpiresAt <= dateTimeProvider.UtcNow)
            return Result.Failure(SessionErrors.RefreshTokenExpired);

        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);
        
        if (currentSession.UserId != userId)
            return Result.Failure(SessionErrors.UnauthorizedAccess);
        
        

        return Result.Success();
    }
}
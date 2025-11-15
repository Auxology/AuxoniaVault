using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.Session;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Authentication;

namespace Auth.Application.Users.RevokeSessions;

internal sealed class RevokeSessionsCommandHandler(IAuthDbContext context, IUserContext userContext, ISessionBlacklistCache sessionBlacklistCache, IDateTimeProvider dateTimeProvider)
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

        List<Session> userSessions = await context.Sessions
            .Where(s => s.UserId == userId && s.Status == SessionStatus.Active)
            .ToListAsync(cancellationToken);

        if (!userSessions.Any())
            return Result.Success();
             
        foreach (Session session in userSessions)
        {
            await sessionBlacklistCache.BlacklistSessionAsync(session.Id.Value, cancellationToken);
        }
        
        context.Sessions.RemoveRange(userSessions);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
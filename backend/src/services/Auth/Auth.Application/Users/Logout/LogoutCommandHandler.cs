using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Aggregates.Session;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.Logout;

internal sealed class LogoutCommandHandler(IAuthDbContext context, IUserContext userContext) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        Guid currentUserId = userContext.UserId;
        
        UserId userId = UserId.UnsafeFromGuid(currentUserId);
        
        Session? session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Token == request.RefreshToken && s.UserId == userId, cancellationToken);
        
        if (session is null)
            return Result.Success();
        
        context.Sessions.Remove(session);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
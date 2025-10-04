using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.Session;
using Auth.Domain.Aggregates.User;
using Auth.Domain.Errors;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.LoginWithRefreshToken;

internal sealed class LoginWithRefreshTokenCommandHandler(IAuthDbContext context, ITokenProvider tokenProvider, IDateTimeProvider dateTimeProvider) : ICommandHandler<LoginWithRefreshTokenCommand, LoginWithRefreshTokenResponse>
{
    public async Task<Result<LoginWithRefreshTokenResponse>> Handle(LoginWithRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Session? session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Token == request.RefreshToken, cancellationToken);

        if (session is null || session.ExpiresAt <= dateTimeProvider.UtcNowForDatabaseComparison() || !session.IsActive(dateTimeProvider))
            return Result.Failure<LoginWithRefreshTokenResponse>(SessionErrors.RefreshTokenExpired);

        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == session.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<LoginWithRefreshTokenResponse>(SessionErrors.UserNotFound);

        string accessToken = tokenProvider.Create(user);

        context.Sessions.Remove(session);

        string newRefreshToken = tokenProvider.CreateRefreshToken();

        Result<Session> newSessionResult = Session.Create
        (
            user.Id,
            newRefreshToken,
            request.requestMetadata.IpAddress,
            request.requestMetadata.UserAgent,
            dateTimeProvider
        );

        if (newSessionResult.IsFailure)
            return Result.Failure<LoginWithRefreshTokenResponse>(newSessionResult.Error);

        context.Sessions.Add(newSessionResult.Value);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginWithRefreshTokenResponse(accessToken, newRefreshToken));
    }
}

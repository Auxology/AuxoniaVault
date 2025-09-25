using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.Session;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.VerifyLogin;

internal sealed class VerifyLoginCommandHandler(IAuthDbContext context, ITokenProvider tokenProvider, IDateTimeProvider dateTimeProvider) : ICommandHandler<VerifyLoginCommand, VerifyLoginResponse>
{
    public async Task<Result<VerifyLoginResponse>> Handle(VerifyLoginCommand request, CancellationToken cancellationToken)
    {
        var emailResult = EmailAddress.Create(request.Email);
        
        if (emailResult.IsFailure)
            return Result.Failure<VerifyLoginResponse>(emailResult.Error);
        
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == emailResult.Value, cancellationToken);
        
        if (user is null)
            return Result.Failure<VerifyLoginResponse>(LoginVerificationErrors.InvalidOrExpired);
        
        var loginVerification = await context.LoginVerifications
            .FirstOrDefaultAsync(lv => lv.Identifier == emailResult.Value && lv.Value == request.Code, cancellationToken);
        
        if (loginVerification is null || loginVerification.ExpiresAt < DateTimeOffset.UtcNow)
            return Result.Failure<VerifyLoginResponse>(LoginVerificationErrors.InvalidOrExpired);

        string token = tokenProvider.Create(user);

        string refreshToken = tokenProvider.CreateRefreshToken();

        Result<Session> sessionResult = Session.Create(user.Id, refreshToken, request.RequestMetadata.IpAddress,
            request.RequestMetadata.UserAgent, dateTimeProvider);
        
        if (sessionResult.IsFailure)
            return Result.Failure<VerifyLoginResponse>(sessionResult.Error);
        
        var response = new VerifyLoginResponse(token, refreshToken);
        
        context.LoginVerifications.Remove(loginVerification);
        context.Sessions.Add(sessionResult.Value);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success(response);
    }
}
using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.VerifyLogin;

internal sealed class VerifyLoginCommandHandler(IAuthDbContext context, ITokenProvider tokenProvider) : ICommandHandler<VerifyLoginCommand, string>
{
    public async Task<Result<string>> Handle(VerifyLoginCommand request, CancellationToken cancellationToken)
    {
        var emailResult = EmailAddress.Create(request.Email);
        
        if (emailResult.IsFailure)
            return Result.Failure<string>(emailResult.Error);
        
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == emailResult.Value, cancellationToken);
        
        if (user is null)
            return Result.Failure<string>(LoginVerificationErrors.InvalidOrExpired);
        
        var loginVerification = await context.LoginVerifications
            .FirstOrDefaultAsync(lv => lv.Identifier == emailResult.Value && lv.Value == request.Code, cancellationToken);
        
        if (loginVerification is null || loginVerification.ExpiresAt < DateTimeOffset.UtcNow)
            return Result.Failure<string>(LoginVerificationErrors.InvalidOrExpired);

        string token = tokenProvider.Create(user);
        
        context.LoginVerifications.Remove(loginVerification);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success(token);
    }
}
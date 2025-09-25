using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.VerifyLogin;

internal sealed class VerifyLoginCommandHandler(IAuthDbContext context) : ICommandHandler<VerifyLoginCommand>
{
    public async Task<Result> Handle(VerifyLoginCommand request, CancellationToken cancellationToken)
    {
        var emailResult = EmailAddress.Create(request.Email);
        
        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);
        
        var loginVerification = await context.LoginVerifications
            .FirstOrDefaultAsync(lv => lv.Identifier == emailResult.Value && lv.Value == request.Code, cancellationToken);
        
        if (loginVerification is null || loginVerification.ExpiresAt < DateTimeOffset.UtcNow)
            return Result.Failure(LoginVerificationErrors.InvalidOrExpired);
        
        context.LoginVerifications.Remove(loginVerification);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
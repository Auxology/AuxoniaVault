using System.Security.Cryptography;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Aggregates.LoginVerification;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.RequestLogin;

internal sealed class RequestLoginCommandHandler(IAuthDbContext context, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RequestLoginCommand>
{
    public async Task<Result> Handle(RequestLoginCommand request, CancellationToken cancellationToken)
    {
        var emailResult = EmailAddress.Create(request.Email);

        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == emailResult.Value, cancellationToken);

        if (user is null)
        {
            Random.Shared.Next(100000, 999999);

            return Result.Success();
        }

        int loginCode = RandomNumberGenerator.GetInt32(100000, 999999);

        Result<LoginVerification> loginResult = LoginVerification.Create(emailResult.Value, loginCode, dateTimeProvider.UtcNow);

        if (loginResult.IsFailure)
            return Result.Failure(loginResult.Error);

        await context.LoginVerifications.AddAsync(loginResult.Value, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
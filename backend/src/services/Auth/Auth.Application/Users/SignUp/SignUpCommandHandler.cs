using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Abstractions.Services;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.User;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.SignUp;

internal sealed class SignUpCommandHandler(IAuthDbContext context, IDateTimeProvider dateTimeProvider, IGenerator generator, ISecretHasher secretHasher) : ICommandHandler<SignUpCommand, string[]>
{
    public async Task<Result<string[]>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);

        if (emailResult.IsFailure)
            return Result.Failure<string[]>(emailResult.Error);

        if (await context.Users.AnyAsync(u => u.Email == emailResult.Value, cancellationToken))
            return Result.Failure<string[]>(UserErrors.EmailNotUnique);

        Result<User> userResult = User.Create(request.Name, emailResult.Value, dateTimeProvider);

        if (userResult.IsFailure)
            return Result.Failure<string[]>(userResult.Error);

        User user = userResult.Value;

        string[] plainRecoveryCodes = await generator.GenerateRecoveryCodesAsync();

        string[] hashedRecoveryCodes = await Task.WhenAll(
            plainRecoveryCodes.Select(code =>
                secretHasher.HashAsync(code, cancellationToken))
        );
        
        Result recoveryCodesResult = user.CreateRecoveryCodes(hashedRecoveryCodes, dateTimeProvider);
        
        if (recoveryCodesResult.IsFailure)
            return Result.Failure<string[]>(recoveryCodesResult.Error);
        
        await context.Users.AddAsync(user, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(plainRecoveryCodes);
    }
}
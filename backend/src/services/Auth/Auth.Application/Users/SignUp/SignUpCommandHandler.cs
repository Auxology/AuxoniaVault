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

internal sealed class SignUpCommandHandler(IAuthDbContext context, IDateTimeProvider dateTimeProvider, IGenerator generator, ISecretHasher secretHasher) : ICommandHandler<SignUpCommand, SignUpCommandResponse>
{
    public async Task<Result<SignUpCommandResponse>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);

        if (emailResult.IsFailure)
            return Result.Failure<SignUpCommandResponse>(emailResult.Error);

        if (await context.Users.AnyAsync(u => u.Email == emailResult.Value, cancellationToken))
            return Result.Failure<SignUpCommandResponse>(UserErrors.EmailNotUnique);

        var metadata = request.RequestMetadata;

        Result<User> userResult = User.Create(request.Name, emailResult.Value, metadata.IpAddress, metadata.UserAgent,
            dateTimeProvider);

        if (userResult.IsFailure)
            return Result.Failure<SignUpCommandResponse>(userResult.Error);

        User user = userResult.Value;

        string[] plainRecoveryCodes = await generator.GenerateRecoveryCodesAsync();

        string[] hashedRecoveryCodes = await Task.WhenAll(
            plainRecoveryCodes.Select(code =>
                secretHasher.HashAsync(code, cancellationToken))
        );
        
        Result recoveryCodesResult = user.CreateRecoveryCodes(hashedRecoveryCodes, dateTimeProvider);
        
        if (recoveryCodesResult.IsFailure)
            return Result.Failure<SignUpCommandResponse>(recoveryCodesResult.Error);
        
        await context.Users.AddAsync(user, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        var response = new SignUpCommandResponse
        (
            UserId: user.Id.Value, 
            RecoveryCodes: plainRecoveryCodes
        );

        return Result.Success(response);
    }
}
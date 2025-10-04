using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.User;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.SignUp;

internal sealed class SignUpCommandHandler(IAuthDbContext context, IDateTimeProvider dateTimeProvider) : ICommandHandler<SignUpCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);

        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        if (await context.Users.AnyAsync(u => u.Email == emailResult.Value, cancellationToken))
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);

        Result<User> userResult = User.Create(request.Name, emailResult.Value, dateTimeProvider);

        if (userResult.IsFailure)
            return Result.Failure<Guid>(userResult.Error);

        User user = userResult.Value;

        await context.Users.AddAsync(user, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id.Value);
    }
}
using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.ChangeName;

internal sealed class ChangeNameCommandHandler(IAuthDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<ChangeNameCommand, string>
{
    public async Task<Result<string>> Handle(ChangeNameCommand request, CancellationToken cancellationToken)
    {
        Guid currentUserId = userContext.UserId;
        
        UserId userId = UserId.UnsafeFromGuid(currentUserId);

        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user is null)
            return Result.Failure<string>(UserErrors.UserNotFound);
        
        Result nameResult = user.ChangeName(request.Name, dateTimeProvider);
        
        if (nameResult.IsFailure)
            return Result.Failure<string>(nameResult.Error);
        
        context.Users.Update(user);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success(user.Name);
    }
}
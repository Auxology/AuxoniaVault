using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Abstractions.Storage;
using Auth.Application.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.SetProfilePicture;

internal sealed class SetProfilePictureCommandHandler(
    IAuthDbContext context,
    IUserContext userContext,
    IStorageServices storageServices,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SetProfilePictureCommand>
{
    public async Task<Result> Handle(SetProfilePictureCommand request, CancellationToken cancellationToken)
    {
        Guid currentUserId = userContext.UserId;

        UserId userId = UserId.UnsafeFromGuid(currentUserId);

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var key = await storageServices.PutObjectAsync(request.File, cancellationToken);

        user.SetProfilePicture(key, dateTimeProvider);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
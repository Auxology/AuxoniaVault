using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Application.Users.GetUser;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.GetUserById;

internal sealed class GetUserByIdQueryHandler(IAuthDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);

        if (request.UserId != userContext.UserId)
            return Result.Failure<UserResponse>(UserErrors.Unauthorized());

        UserResponse? user = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserResponse
            (
                u.Id.Value,
                u.Email.Value,
                u.Name,
                u.Avatar,
                u.CreatedAt,
                u.UpdatedAt
            ))
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        return Result.Success(user);
    }
}
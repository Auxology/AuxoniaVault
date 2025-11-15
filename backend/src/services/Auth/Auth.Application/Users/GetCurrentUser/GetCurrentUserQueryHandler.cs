using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.User;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.GetCurrentUser;

internal sealed class GetCurrentUserQueryHandler(IAuthDbContext context, IUserContext userContext) : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);
        
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure<CurrentUserResponse>(UserErrors.UserNotFound);

        CurrentUserResponse response = new
        (
            Id: user.Id.Value,
            Email: user.Email.Value,
            Name: user.Name
        );
        
        return Result.Success(response);
    }
}
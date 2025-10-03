using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.GetUser;

public record GetUserByIdQuery
(
    Guid UserId
) : IQuery<UserResponse>;
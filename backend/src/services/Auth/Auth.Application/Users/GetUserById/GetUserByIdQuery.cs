using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.GetUserById;

public record GetUserByIdQuery
(
    Guid UserId
) : IQuery<UserResponse>;
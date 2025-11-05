using Auth.Application.Abstractions.Messaging;
using Auth.Application.Users.GetUserById;

namespace Auth.Application.Users.GetUser;

public record GetUserByIdQuery
(
    Guid UserId
) : IQuery<UserResponse>;
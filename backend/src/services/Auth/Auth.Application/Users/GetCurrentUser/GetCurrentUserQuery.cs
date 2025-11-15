using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.GetCurrentUser;

public sealed record GetCurrentUserQuery() : IQuery<CurrentUserResponse>;
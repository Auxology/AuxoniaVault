using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.RevokeSessions;

public record RevokeSessionsCommand
(
    string RefreshToken
) : ICommand;
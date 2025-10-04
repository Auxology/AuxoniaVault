using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.Logout;

public record LogoutCommand
(
    string RefreshToken
) : ICommand;
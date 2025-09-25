using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.RequestLogin;

public record RequestLoginCommand(string Email) : ICommand;
using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.SignUp;

public record SignUpCommand
(
    string Name,
    string Email
) : ICommand<string[]>;
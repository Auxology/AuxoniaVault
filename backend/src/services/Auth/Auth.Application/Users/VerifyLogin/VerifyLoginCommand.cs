using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.VerifyLogin;

public record VerifyLoginCommand
(
    string Email,
    int Code
) : ICommand<string>;
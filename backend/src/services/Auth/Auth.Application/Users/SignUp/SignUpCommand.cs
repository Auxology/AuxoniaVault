using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.SignUp;

public record SignUpCommand
(
    string Name,
    string Email,
    RequestMetadata RequestMetadata
) : ICommand<string[]>;
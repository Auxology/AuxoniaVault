using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.SignUp;

public sealed record SignUpCommand
(
    string Name,
    string Email,
    RequestMetadata RequestMetadata
) : ICommand<SignUpCommandResponse>;

public sealed record SignUpCommandResponse
(
    Guid UserId,
    IReadOnlyList<string> RecoveryCodes
);
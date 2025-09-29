using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.RequestEmailChange;

public record RequestEmailChangeCommand
(
    string Email,
    RequestMetadata RequestMetadata
) : ICommand;
using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.RequestRecovery;

public record RequestRecoveryCommand
(
    Guid UserId,
    string RecoveryCode,
    RequestMetadata RequestMetadata
) : ICommand<string>;
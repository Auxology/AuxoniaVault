using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.RequestRecovery;

public record RequestRecoveryCommand
(
    Guid UserId,
    string RecoveryCode
) : ICommand<string>;
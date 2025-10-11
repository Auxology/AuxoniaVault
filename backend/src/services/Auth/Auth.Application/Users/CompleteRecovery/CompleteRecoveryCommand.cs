using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.CompleteRecovery;

public record CompleteRecoveryCommand
(
    string NewEmail,
    string UniqueIdentifier
): ICommand;
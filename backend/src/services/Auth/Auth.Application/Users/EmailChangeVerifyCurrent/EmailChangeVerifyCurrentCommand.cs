using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.EmailChangeVerifyCurrent;

public record EmailChangeVerifyCurrentCommand
(
    int CurrentOtp
) : ICommand;
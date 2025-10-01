using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.EmailChangeVerifyNew;

public record EmailChangeVerifyNewCommand
(
    int NewOtp
) : ICommand;
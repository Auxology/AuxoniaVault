using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.EmailChangeVerifyCurrent;

public record EmailChangeVerifyCurrentCommand
(
    int CurrentOtp,
    RequestMetadata RequestMetadata
) : ICommand;
using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.EmailChangeVerifyNew;

public record EmailChangeVerifyNewCommand
(
    int NewOtp,
    RequestMetadata RequestMetadata
) : ICommand;
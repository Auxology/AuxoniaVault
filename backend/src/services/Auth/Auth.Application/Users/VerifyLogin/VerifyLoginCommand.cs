using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.VerifyLogin;

public record VerifyLoginCommand
(
    string Email,
    int Code,
    RequestMetadata RequestMetadata
) : ICommand<VerifyLoginResponse>;

public record VerifyLoginResponse
(
    string AccessToken,
    string RefreshToken
);
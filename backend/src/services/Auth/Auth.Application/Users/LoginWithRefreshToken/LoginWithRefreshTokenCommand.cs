using Auth.Application.Abstractions.LoggingInfo;
using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.LoginWithRefreshToken;

public record LoginWithRefreshTokenCommand
(
    string RefreshToken,
    RequestMetadata requestMetadata
) : ICommand<LoginWithRefreshTokenResponse>;

public record LoginWithRefreshTokenResponse
(
    string AccessToken,
    string RefreshToken
);
namespace Shared.Responses;

public sealed record LoginWithRefreshTokenResponse
(
    string AccessToken,
    string RefreshToken
);
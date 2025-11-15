namespace Shared.Responses;

public sealed record VerifyLoginResponse
(
    string AccessToken,
    string RefreshToken
);
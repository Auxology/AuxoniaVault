namespace Shared.Requests;

public sealed record VerifyLoginResponse
(
    string AccessToken,
    string RefreshToken
);
namespace Shared.Requests;

public sealed record VerifyLoginRequest
(
    string Email,
    int Code
);
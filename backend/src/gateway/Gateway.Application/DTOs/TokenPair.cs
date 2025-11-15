namespace Gateway.Application.DTOs;

public sealed record TokenPair
(
    string AccessToken,
    string RefreshToken
);
namespace Auth.Application.Users.GetCurrentUser;

public sealed record class CurrentUserResponse
(
    Guid Id,
    string Email,
    string Name
);
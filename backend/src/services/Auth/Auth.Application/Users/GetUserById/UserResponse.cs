namespace Auth.Application.Users.GetUserById;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string Name,
    string? AvatarUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
using FluentValidation;

namespace Auth.Application.Users.LoginWithRefreshToken;

internal sealed class LoginWithRefreshTokenValidator : AbstractValidator<LoginWithRefreshTokenCommand>
{
    public LoginWithRefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token must not be empty.");
    }
}
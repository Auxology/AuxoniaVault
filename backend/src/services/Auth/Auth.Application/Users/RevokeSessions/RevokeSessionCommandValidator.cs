using FluentValidation;

namespace Auth.Application.Users.RevokeSessions;

internal sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionsCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token must not be empty.");
    }
}
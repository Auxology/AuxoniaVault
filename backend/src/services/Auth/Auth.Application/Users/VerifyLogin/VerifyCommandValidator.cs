using FluentValidation;

namespace Auth.Application.Users.VerifyLogin;

internal sealed class VerifyCommandValidator : AbstractValidator<VerifyLoginCommand>
{
    public VerifyCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.");
    }
}
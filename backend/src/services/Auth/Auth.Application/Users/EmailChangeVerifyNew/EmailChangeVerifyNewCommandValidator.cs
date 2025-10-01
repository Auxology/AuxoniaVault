using FluentValidation;

namespace Auth.Application.Users.EmailChangeVerifyNew;

internal sealed class EmailChangeVerifyNewCommandValidator: AbstractValidator<EmailChangeVerifyNewCommand>
{
    public EmailChangeVerifyNewCommandValidator()
    {
        RuleFor(x => x.NewOtp)
            .NotEmpty().WithMessage("Verification code is required.")
            .InclusiveBetween(100000, 999999).WithMessage("Verification code must be a 6-digit number.");
    }
}
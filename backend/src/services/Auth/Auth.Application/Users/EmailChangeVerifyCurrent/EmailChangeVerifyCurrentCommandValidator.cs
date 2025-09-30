using FluentValidation;

namespace Auth.Application.Users.EmailChangeVerifyCurrent;

internal sealed class EmailChangeVerifyCurrentCommandValidator : AbstractValidator<EmailChangeVerifyCurrentCommand>
{
    public EmailChangeVerifyCurrentCommandValidator()
    {
        RuleFor(x => x.CurrentOtp)
            .NotEmpty().WithMessage("Verification code is required.")
            .InclusiveBetween(100000, 999999).WithMessage("Verification code must be a 6-digit number.");
    }
}
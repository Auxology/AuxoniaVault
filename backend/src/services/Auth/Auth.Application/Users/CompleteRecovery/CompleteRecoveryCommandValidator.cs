using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.CompleteRecovery;

internal sealed class CompleteRecoveryCommandValidator : AbstractValidator<CompleteRecoveryCommand>
{
    public CompleteRecoveryCommandValidator()
    {
        RuleFor(crc => crc.NewEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(UserConstants.MaxEmailLength).WithMessage($"Email cannot be longer than {UserConstants.MaxEmailLength} characters.");

        RuleFor(crc => crc.UniqueIdentifier)
            .NotEmpty().WithMessage("Unique identifier is required.");
    }
}
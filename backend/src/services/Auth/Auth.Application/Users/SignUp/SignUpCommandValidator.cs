using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using FluentValidation;

namespace Auth.Application.Users.SignUp;

internal sealed class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator()
    {
        RuleFor(sc => sc.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(UserConstants.MaxEmailLength).WithMessage($"Email cannot be longer than {UserConstants.MaxEmailLength} characters.");

        RuleFor(sc => sc.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(UserConstants.MaxNameLength).WithMessage($"Name cannot be longer than {UserConstants.MaxNameLength} characters.");
    }
}
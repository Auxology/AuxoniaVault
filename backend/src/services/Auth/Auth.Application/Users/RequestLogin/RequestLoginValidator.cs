using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.RequestLogin;

internal sealed class RequestLoginValidator : AbstractValidator<RequestLoginCommand>
{
    public RequestLoginValidator()
    {
        RuleFor(rl => rl.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(UserConstants.MaxEmailLength).WithMessage($"Email cannot be longer than {UserConstants.MaxEmailLength} characters.");
    }
}
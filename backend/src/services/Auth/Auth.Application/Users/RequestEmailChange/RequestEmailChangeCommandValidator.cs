using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.RequestEmailChange;

internal sealed class RequestEmailChangeCommandValidator : AbstractValidator<RequestEmailChangeCommand>
{
    public RequestEmailChangeCommandValidator()
    {
        RuleFor(rec => rec.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(UserConstants.MaxEmailLength).WithMessage($"Email cannot be longer than {UserConstants.MaxEmailLength} characters.");
    }
}
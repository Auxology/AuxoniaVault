using FluentValidation;

namespace Auth.Application.Users.RequestLogin;

internal sealed class RequestLoginValidator : AbstractValidator<RequestLoginCommand>
{
    public RequestLoginValidator()
    {
        RuleFor(rl => rl.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");
    }
}
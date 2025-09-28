using Auth.Domain.Constants;
using FluentValidation;

namespace Auth.Application.Users.ChangeName;

internal sealed class ChangeNameCommandValidator : AbstractValidator<ChangeNameCommand>
{
    public ChangeNameCommandValidator()
    {
        RuleFor(cnc => cnc.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(UserConstants.MaxNameLength).WithMessage($"Name cannot be longer than {UserConstants.MaxNameLength} characters.");
    }
}
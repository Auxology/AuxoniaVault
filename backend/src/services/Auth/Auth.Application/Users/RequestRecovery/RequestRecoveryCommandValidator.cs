using FluentValidation;

namespace Auth.Application.Users.RequestRecovery;

internal sealed class RequestRecoveryCommandValidator : AbstractValidator<RequestRecoveryCommand>
{
    public RequestRecoveryCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .NotEqual(Guid.Empty).WithMessage("UserId cannot be an empty GUID.");

        RuleFor(x => x.RecoveryCode)
            .NotEmpty().WithMessage("RecoveryCode is required.");
    }
}
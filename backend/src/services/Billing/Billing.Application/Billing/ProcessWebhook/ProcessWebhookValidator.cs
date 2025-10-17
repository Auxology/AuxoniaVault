using FluentValidation;

namespace Billing.Application.Billing.ProcessWebhook;

internal sealed class ProcessWebhookValidator : AbstractValidator<ProcessWebhookCommand>
{
    public ProcessWebhookValidator()
    {
        RuleFor(x => x.EventJson)
            .NotEmpty().WithMessage("Event JSON must not be empty.");

        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Signature must not be empty.");
    }
}
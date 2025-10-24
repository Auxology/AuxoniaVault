using FluentValidation;

namespace Billing.Application.Webhooks.ProcessSubscriptionUpdated;

internal sealed class ProcessSubscriptionUpdatedCommandValidator 
    : AbstractValidator<ProcessSubscriptionUpdatedCommand>
{
    public ProcessSubscriptionUpdatedCommandValidator()
    {
        RuleFor(x => x.StripeCustomerId)
            .NotEmpty()
            .WithMessage("Stripe customer ID is required.");

        RuleFor(x => x.StripeSubscriptionId)
            .NotEmpty()
            .WithMessage("Stripe subscription ID is required.");

        RuleFor(x => x.CurrentPeriodStart)
            .LessThan(x => x.CurrentPeriodEnd)
            .WithMessage("Current period start must be before current period end.");
    }
}
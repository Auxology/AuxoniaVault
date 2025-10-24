using FluentValidation;

namespace Billing.Application.Billing.CancelSubscription;

internal sealed class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(csc => csc.StripeSubscriptionId)
            .NotEmpty().WithMessage("Stripe Subscription ID cannot be empty")
            .Matches(@"^sub_[a-zA-Z0-9]+$").WithMessage("Stripe Subscription ID is not valid");
    }
}
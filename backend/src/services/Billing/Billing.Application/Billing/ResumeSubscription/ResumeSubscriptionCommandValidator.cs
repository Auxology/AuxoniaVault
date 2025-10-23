using FluentValidation;

namespace Billing.Application.Billing.ResumeSubscription;

internal sealed class ResumeSubscriptionCommandValidator : AbstractValidator<ResumeSubscriptionCommand>
{
    public ResumeSubscriptionCommandValidator()
    {
        RuleFor(rsc => rsc.StripeSubscriptionId)
            .NotEmpty().WithMessage("Stripe Subscription ID cannot be empty")
            .Matches(@"^sub_[a-zA-Z0-9]+$").WithMessage("Stripe Subscription ID is not valid");
    }
}
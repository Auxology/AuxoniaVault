using FluentValidation;

namespace Billing.Application.Webhooks.ProcessSubscriptionDeleted;

internal sealed class ProcessSubscriptionDeletedCommandValidator : AbstractValidator<ProcessSubscriptionDeletedCommand>
{
    public ProcessSubscriptionDeletedCommandValidator()
    {
        RuleFor(x => x.StripeCustomerId)
            .NotEmpty()
            .WithMessage("Stripe customer ID is required.");

        RuleFor(x => x.StripeSubscriptionId)
            .NotEmpty()
            .WithMessage("Stripe subscription ID is required.");
        
        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required.");

        RuleFor(x => x.PriceFormatted)
            .NotEmpty()
            .WithMessage("Price formatted is required.");

        RuleFor(x => x.CurrentPeriodStart)
            .LessThan(x => x.CurrentPeriodEnd)
            .WithMessage("Current period start must be before current period end.");
    } 
}
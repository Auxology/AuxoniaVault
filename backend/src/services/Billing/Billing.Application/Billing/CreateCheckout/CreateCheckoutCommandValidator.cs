using FluentValidation;

namespace Billing.Application.Billing.CreateCheckout;

internal sealed class CreateCheckoutCommandValidator : AbstractValidator<CreateCheckoutCommand>
{
    public CreateCheckoutCommandValidator()
    {
        RuleFor(ccc => ccc.PriceId)
            .NotEmpty().WithMessage("Price Id cannot be empty")
            .Matches(@"^price_[a-zA-Z0-9]+$").WithMessage("Price Id is not valid");
    }
}
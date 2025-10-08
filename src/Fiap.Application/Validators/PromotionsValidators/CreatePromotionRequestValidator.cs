namespace Fiap.Application.Validators.PromotionsValidators
{
    [ExcludeFromCodeCoverage]
    public class CreatePromotionRequestValidator : AbstractValidator<CreatePromotionRequest>
    {
        public CreatePromotionRequestValidator()
        {
            RuleFor(x => x.Discount)
                .NotEmpty()
                .WithMessage("Discount is required.")
                .GreaterThan(0)
                .WithMessage("Discount must be greater than 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Discount must be less than or equal to 100.");

            RuleFor(x => x.ExpirationDate)
                .NotEmpty()
                .WithMessage("Expiration date is required.")
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Expiration date must be in the future.");

            RuleFor(x => x.GameId)
                .Cascade(CascadeMode.Stop)
                .Must(list => list == null || list.All(id => id.HasValue && id.Value > 0))
                .WithMessage("All GameIds must be non-null.");
        }
    }
}

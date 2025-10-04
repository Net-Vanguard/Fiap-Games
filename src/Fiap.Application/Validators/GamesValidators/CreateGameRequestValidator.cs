namespace Fiap.Application.Validators.GamesValidators
{
    [ExcludeFromCodeCoverage]
    public class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
    {
        public CreateGameRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Game name is required.")
                .Matches(@"^[A-Za-z0-9\s]+$")
                .WithMessage("Game name can only contain letters, numbers, and spaces.")
                .MaximumLength(100)
                .WithMessage("Game name must be at most 100 characters long.");

            RuleFor(x => x.Genre)
                .NotEmpty()
                .WithMessage("Genre is required.")
                .Matches(@"^[A-Za-z\s]+$")
                .WithMessage("Genre can only contain letters and spaces.")
                .MaximumLength(100)
                .WithMessage("Genre must be at most 100 characters long.");

            RuleFor(x => x.Price)
                .NotEmpty()
                .WithMessage("Price is required.")
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.")
                .LessThanOrEqualTo(1000)
                .WithMessage("Price must be less than or equal to 1000.");

            RuleFor(x => x.PromotionId)
                .GreaterThan(0)
                .When(x => x.PromotionId.HasValue)
                .WithMessage("PromotionId must be greater than 0 when provided.");
        }
    }
}
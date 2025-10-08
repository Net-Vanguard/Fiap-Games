namespace Fiap.Domain.GameAggregate
{
    public class Game : Entity, IAggregateRoot
	{
		public Game(int id, string name, string genre, decimal price, int? promotionId, string currency = "BRL")
            : this(name, genre, price, promotionId)
        {
            Id = id;
        }

        public Game(string name, string genre, decimal price, int? promotionId, string currency = "BRL")
        {
            ValidateName(name);
            ValidateGenre(genre);

            Name = name;
            Genre = genre;
            Price = new Money(price, currency);
            PromotionId = promotionId;
        }

        public Game() { }

        public string Name { get; set; }
        public string Genre { get; set; }
        public Money Price { get; set; }
        public int? PromotionId { get; set; }

        public virtual Promotion Promotion { get; set; }

        public static Game Create(string name, string genre, decimal price, int? promotionId, string currency = "BRL")
        {
            var game = new Game
            {
                Name = name,
                Genre = genre,
                Price = new Money(price, currency),
                PromotionId = promotionId
            };

            return game;
        }

        public void AssignPromotion(int promotionId)
        {
            PromotionId = promotionId;
        }

        public void RemovePromotion()
        {
            PromotionId = null;
            Promotion = null;
        }

        public decimal GetFinalPrice()
        {
            if (!PromotionId.HasValue)
                return Price.Value;

            if (Promotion != null && Promotion.IsActive())
            {
                return Promotion.GetDiscountedPrice(Price.Value);
            }

            return Price.Value;
        }

        public bool HasActivePromotion()
        {
            return PromotionId.HasValue && 
                   (Promotion == null || Promotion.IsActive());
        }

        public decimal GetDiscountAmount()
        {
            if (!HasActivePromotion())
                return 0;

            return Price.Value - GetFinalPrice();
        }

        public decimal GetDiscountPercentage()
        {
            if (!HasActivePromotion())
                return 0;

            return Promotion.Discount.Value;
        }
        private void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRulesException("The name of the game is required.");
        }
        private void ValidateGenre(string genre)
        {
            if (string.IsNullOrWhiteSpace(genre))
                throw new BusinessRulesException("The genre of the game is required.");
        }
	}
}

namespace Fiap.Application.Games.Models.Response
{
    [ExcludeFromCodeCoverage]
    public record GameResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public int? PromotionId { get; set; }
        public decimal PriceWithDiscount { get; set; }

        public static explicit operator GameResponse(Game game)
        {
            var priceWithDiscount = game.Price.Value;
                        
            if (game.Promotion is not null)
            {
                var now = DateTime.UtcNow;
                var startDate = game.Promotion.StartDate?.Value;
                var endDate = game.Promotion.EndDate?.Value;
                var isActive = game.Promotion.IsActive();
                                
                if (isActive)
                {
                    priceWithDiscount = game.Price.Value * (1 - game.Promotion.Discount.Value / 100);
                }
            }
            
            return new GameResponse
            {
                Id = game.Id,
                Name = game.Name,
                Genre = game.Genre,
                Price = game.Price.Value,
                PromotionId = game.PromotionId,
                PriceWithDiscount = priceWithDiscount
            };
        }
    }
}
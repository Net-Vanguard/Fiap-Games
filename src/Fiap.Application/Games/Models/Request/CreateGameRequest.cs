
namespace Fiap.Application.Games.Models.Request
{
    [ExcludeFromCodeCoverage]
    public class CreateGameRequest
    {
        public string Name { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public int? PromotionId { get; set; }

        public static explicit operator Game(CreateGameRequest c)
        {
            return new Game(
                name: c.Name,
                genre: c.Genre,
                price: c.Price,
                promotionId: c.PromotionId
            );
        }
    }
}

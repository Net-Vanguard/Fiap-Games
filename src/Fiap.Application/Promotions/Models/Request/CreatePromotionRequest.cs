namespace Fiap.Application.Promotions.Models.Request
{
    [ExcludeFromCodeCoverage]
    public record CreatePromotionRequest
    {
        public decimal Discount { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<int?>? GameId { get; set; }

        public static explicit operator Promotion(CreatePromotionRequest c)
        {
            return new Promotion
            {
                Discount = new Money(c.Discount),
                EndDate = new UtcDate(c.ExpirationDate),
                StartDate = new UtcDate(DateTime.UtcNow),
            };
        }
    }
}

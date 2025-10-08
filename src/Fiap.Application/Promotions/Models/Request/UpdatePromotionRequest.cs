namespace Fiap.Application.Promotions.Models.Request
{
    [ExcludeFromCodeCoverage]
    public record UpdatePromotionRequest
    {
        public decimal? Discount { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public List<int?>? GameId { get; set; }
    }
}

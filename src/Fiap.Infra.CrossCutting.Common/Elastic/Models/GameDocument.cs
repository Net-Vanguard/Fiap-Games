namespace Fiap.Infra.CrossCutting.Common.Elastic.Models
{
    public class GameDocument
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? PromotionId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public bool HasActivePromotion { get; set; }
        public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
        public int PopularityScore { get; set; } = 0;
        public List<string> Tags { get; set; } = new();
    }
}

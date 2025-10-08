namespace Fiap.Application.Promotions.Models.Response
{
    [ExcludeFromCodeCoverage]
    public record PromotionResponse
    {
        public int PromotionId { get; set; }
        public decimal Discount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public static explicit operator PromotionResponse(Promotion promotion)
        {
            return new PromotionResponse
            {
                PromotionId = promotion.Id,
                Discount = promotion.Discount.Value,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate
            };
        }
    }
}

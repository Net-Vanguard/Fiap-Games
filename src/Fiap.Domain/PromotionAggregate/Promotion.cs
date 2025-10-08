namespace Fiap.Domain.PromotionAggregate
{
    public class Promotion : Entity, IAggregateRoot
    {
        public Promotion()
        {
        }

        public Promotion(decimal discount, DateTime startDate, DateTime endDate)
        {
            Discount = new Money(discount);
            StartDate = new UtcDate(startDate);
            EndDate = new UtcDate(endDate);
        }

        public Money Discount { get; set; }
        public UtcDate StartDate { get; set; }
        public UtcDate EndDate { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Game> Games { get; set; } = [];
		public void UpdateDiscount(decimal? discount, DateTime? endDate)
        {
            if (discount.HasValue)
                Discount = new Money(discount.Value);

            if (endDate.HasValue)
                EndDate = new UtcDate(endDate.Value);
        }

        public void ValidatePeriod()
        {
            if (EndDate <= StartDate.Value)
                throw new BusinessRulesException("Promotion end date cannot be earlier than the start date.");
        }

        public bool IsExpired() => EndDate < DateTime.UtcNow;

        public bool IsActive() => StartDate <= DateTime.UtcNow && EndDate >= DateTime.UtcNow;

        public decimal GetDiscountedPrice(decimal originalPrice)
        {
            return originalPrice * (1 - Discount.Value / 100);
        }
	}
}

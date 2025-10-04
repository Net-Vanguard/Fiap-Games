using Microsoft.EntityFrameworkCore;

namespace Fiap.Infra.Data.MapEntities
{
    public class PromotionsMap : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.ToTable("Promotions");

            builder.HasKey(x => x.Id);

            builder.OwnsOne(p => p.Discount, discount =>
            {
                discount.Property(d => d.Value)
                        .HasColumnName("DiscountValue")
                        .IsRequired();

                discount.Property(d => d.Currency)
                        .HasColumnName("DiscountCurrency")
                        .IsRequired()
                        .HasMaxLength(3);

                discount.HasData(
                    new { PromotionId = 1, Value = 10.15M, Currency = "USD" },
                    new { PromotionId = 2, Value = 15.98M, Currency = "USD" },
                    new { PromotionId = 3, Value = 20.97M, Currency = "USD" }
                );
            });

            builder.Property(p => p.StartDate)
                   .HasConversion(
                       v => v.Value,
                       v => new UtcDate(DateTime.SpecifyKind(v, DateTimeKind.Utc)))
                   .IsRequired();

            builder.Property(p => p.EndDate)
                   .HasConversion(
                       v => v.Value,
                       v => new UtcDate(DateTime.SpecifyKind(v, DateTimeKind.Utc)))
                   .IsRequired();

            builder.HasData(PromotionSeed.Promotions());
        }
    }
}

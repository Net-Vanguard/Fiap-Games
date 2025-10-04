namespace Fiap.Infra.Data.MapEntities
{
    public class GamesMap : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.ToTable("Games");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("IX_Games_Name");

            builder.HasIndex(x => x.PromotionId)
                .HasDatabaseName("IX_Games_PromotionId");

            builder.Property(x => x.Genre)
                .IsRequired()
                .HasMaxLength(100);

            builder.OwnsOne(x => x.Price, price =>
            {
                price.Property(p => p.Value)
                     .HasColumnName("Price")
                     .IsRequired();

                price.Property(p => p.Currency)
                     .HasColumnName("PriceCurrency")
                     .IsRequired()
                     .HasMaxLength(3);

            });

            builder.HasOne(x => x.Promotion)
                   .WithMany(p => p.Games)
                   .HasForeignKey(x => x.PromotionId);

            builder.HasData(GameSeed.Game());

            builder.OwnsOne(x => x.Price).HasData(
                new { GameId = 1, Value = 299.00M, Currency = "USD" },
                new { GameId = 2, Value = 39.99M, Currency = "BRL" },
                new { GameId = 3, Value = 49.99M, Currency = "BRL" },
                new { GameId = 4, Value = 29.99M, Currency = "BRL" },
                new { GameId = 5, Value = 39.99M, Currency = "BRL" },
                new { GameId = 6, Value = 26.95M, Currency = "BRL" },
                new { GameId = 7, Value = 39.99M, Currency = "BRL" },
                new { GameId = 8, Value = 49.99M, Currency = "BRL" }
            );
        }
    }
}

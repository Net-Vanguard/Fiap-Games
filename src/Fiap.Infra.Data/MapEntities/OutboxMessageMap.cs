namespace Fiap.Infra.Data.MapEntities
{
    public class OutboxMessageMap : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Content)
                   .IsRequired();

            builder.Property(x => x.OccuredOn)
                   .IsRequired();

            builder.Property(x => x.ProcessedOn)
                   .IsRequired(false);
        }
    }
}

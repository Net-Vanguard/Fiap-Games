namespace Fiap.Infra.Data.MapEntities.Seeds
{
    public static class PromotionSeed
    {
        public static List<Promotion> Promotions()
        {
            return
            [
                new Promotion
                {
                    Id = 1,
                    StartDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 04, 01), DateTimeKind.Utc)),
                    EndDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 05, 01), DateTimeKind.Utc)),
                },
                new Promotion
                {
                    Id = 2,
                    StartDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 06, 01), DateTimeKind.Utc)),
                    EndDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 07, 01), DateTimeKind.Utc)),
                },
                new Promotion
                {
                    Id = 3,
                    StartDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 08, 01), DateTimeKind.Utc)),
                    EndDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 09, 01), DateTimeKind.Utc)),
                }
            ];
        }
    }
}

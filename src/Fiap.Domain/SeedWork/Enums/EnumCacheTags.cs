namespace Fiap.Domain.SeedWork.Enums
{
    public static class EnumCacheTags
    {
        public static string FiapPrefix => "Fiap:";

        // Users
        public static string UsersPrefix => $"{FiapPrefix}Users:";
        public static string AllUsers => $"{UsersPrefix}All";
        public static string UserId(int id) => $"{UsersPrefix}Id:{id}";
        public static string UserGames(int userId) => $"{UsersPrefix}Id:{userId}:Games";

        // Games
        public static string GamesPrefix => $"{FiapPrefix}Games:";
        public static string AllGames => $"{GamesPrefix}All";
        public static string GameId(int id) => $"{GamesPrefix}Id:{id}";

        // Promotions
        public static string PromotionsPrefix => $"{FiapPrefix}Promotions:";
        public static string AllPromotions => $"{PromotionsPrefix}All";
        public static string PromotionId(int id) => $"{PromotionsPrefix}Id:{id}";
    }
}

namespace Fiap.Infra.Data.MapEntities.Seeds
{
    public static class GameSeed
    {
        public static List<Game> Game()
        {
            return [
                    new Game { Id = 1, Name = "The Legend of Zelda: Breath of the Wild", Genre = "Action RPG" },
                    new Game { Id = 2, Name = "The Witcher 3: Wild Hunt", Genre = "Action RPG" },
                    new Game { Id = 3, Name = "Red Dead Redemption 2", Genre = "Action-adventure"},
                    new Game { Id = 4, Name = "Dark Souls III", Genre = "Action RPG" },
                    new Game { Id = 5, Name = "God of War", Genre = "Action-adventure" },
                    new Game { Id = 6, Name = "Minecraft", Genre = "Sandbox" },
                    new Game { Id = 7, Name = "Overwatch", Genre = "First-person shooter" },
                    new Game { Id = 8, Name = "The Last of Us Part II", Genre = "Action-adventure" }
            ];
        }
    }
}

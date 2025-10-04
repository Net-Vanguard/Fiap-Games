namespace Fiap.Unit.Tests._3._Domain_Layer_Tests.Entities
{
    public class GameDomainTest
    {
        [Fact]
        public void ParameterlessConstructor_ShouldCreateInstance()
        {
            var game = new Game();

            Assert.NotNull(game);
            Assert.Null(game.Name);
            Assert.Null(game.Genre);
            Assert.Equal(null, game.Price);
            Assert.Null(game.PromotionId);
            Assert.Null(game.Promotion); 
        }

        [Fact]
        public void Constructor_WithBasicParameters_ShouldSetProperties()
        {
            var game = new Game("Game X", "Action", 49.99M, null);

            Assert.Equal("Game X", game.Name);
            Assert.Equal("Action", game.Genre);
            Assert.Equal(49.99M, game.Price.Value);
            Assert.Equal("BRL", game.Price.Currency);
            Assert.Null(game.PromotionId);
        }

        [Fact]
        public void Constructor_WithId_ShouldSetAllProperties()
        {
            var game = new Game(10, "Game With ID", "RPG", 59.99M, 5);

            Assert.Equal(10, game.Id);
            Assert.Equal("Game With ID", game.Name);
            Assert.Equal("RPG", game.Genre);
            Assert.Equal(59.99M, game.Price.Value);            
            Assert.Equal(5, game.PromotionId);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(-9999999999999999)]
        public void Constructor_ShouldThrowException_WhenPriceIsNegative(decimal price)
        {
            var ex = Assert.Throws<BusinessRulesException>(() =>
                new Game("Game", "Action", price, null));

            Assert.Equal("The price must be greater than or equal to 0.", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldAllow_WhenPriceIsZero()
        {
            var game = new Game("Free Game", "Indie", 0, null);

            Assert.Equal(0, game.Price.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrowException_WhenNameIsInvalid(string name)
        {
            var ex = Assert.Throws<BusinessRulesException>(() =>
                new Game(name, "Action", 49.99M, null));

            Assert.Equal("The name of the game is required.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrowException_WhenGenreIsInvalid(string genre)
        {
            var ex = Assert.Throws<BusinessRulesException>(() =>
                new Game("Game", genre, 49.99M, null));

            Assert.Equal("The genre of the game is required.", ex.Message);
        }

        [Fact]
        public void AssignPromotion_ShouldSetPromotionId()
        {
            var game = new Game("Game", "Puzzle", 19.99M, null);
            game.AssignPromotion(5);

            Assert.Equal(5, game.PromotionId);
        }

        [Fact]
        public void Promotion_Property_ShouldWorkCorrectly()
        {
            var promotion = new Promotion(20, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
            var game = new Game { Promotion = promotion };

            Assert.NotNull(game.Promotion);
            Assert.Equal(20, game.Promotion.Discount.Value);
        }

        [Fact]
        public void PromotionId_ShouldWork_WithoutPromotionObject()
        {
            var game = new Game { PromotionId = 5 };

            Assert.Equal(5, game.PromotionId);
            Assert.Null(game.Promotion);
        }

        [Fact]
        public void Should_Handle_Null_Promotion()
        {
            var game = new Game { Promotion = null };

            Assert.Null(game.Promotion);
        }

   
        [Fact]
        public void Constructor_ShouldHandle_MaxDoublePrice()
        {
            var game = new Game("Expensive Game", "AAA", decimal.MaxValue, null);

            Assert.Equal(decimal.MaxValue, game.Price.Value);
        }

        [Fact]
        public void Constructor_ShouldHandle_LongStrings()
        {
            var game = new Game(new string('A', 1000), new string('B', 500), 59.99M, null);

            Assert.Equal(1000, game.Name.Length);
            Assert.Equal(500, game.Genre.Length);
        }

        [Fact]
        public void ValidatePrice_ShouldPass_ForValidValues()
        {
            var game1 = new Game("Game1", "Action", 0, null);
            var game2 = new Game("Game2", "Action", 10.5M, null);
            var game3 = new Game("Game3", "Action", decimal.MaxValue, null);

            Assert.Equal(0, game1.Price.Value);
            Assert.Equal(10.5M, game2.Price.Value);
            Assert.Equal(decimal.MaxValue, game3.Price.Value);
        }

        [Fact]
        public void Promotion_ShouldUpdateCorrectly_WhenReassigned()
        {
            var game = new Game();
            var promotion1 = new Promotion(10, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
            var promotion2 = new Promotion(20, DateTime.UtcNow, DateTime.UtcNow.AddDays(2));

            game.Promotion = promotion1;
            game.Promotion = promotion2;

            Assert.Equal(20, game.Promotion.Discount.Value);
        }

      
        [Fact]
        public void Promotion_Setter_ShouldHandleReassignment()
        {
            var game = new Game();
            var promo1 = new Promotion(10, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
            var promo2 = new Promotion(20, DateTime.UtcNow, DateTime.UtcNow.AddDays(2));

            game.Promotion = promo1;
            game.Promotion = promo2;

            Assert.Equal(20, game.Promotion.Discount.Value);
        }

        [Fact]
        public void CreateGame_ShouldSetPriceWithCurrency()
        {
            // Arrange
            var name = "Test Game";
            var genre = "Action";
            var price = 99.99M;
            var promotionId = (int?)null;

            // Act
            var game = new Game(name, genre, price, promotionId);

            // Assert
            Assert.Equal(name, game.Name);
            Assert.Equal(genre, game.Genre);
            Assert.Equal(price, game.Price.Value);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenCurrencyIsInvalid()
        {
            var ex = Assert.Throws<BusinessRulesException>(() =>
                new Game("Game", "Action", 49.99M, null, "INVALID"));

            Assert.Equal("Invalid currency: INVALID. Supported currencies are: USD, EUR, BRL, JPY, GBP", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldAllow_WhenCurrencyIsValid()
        {
            var game = new Game("Game", "Action", 49.99M, null, "USD");

            Assert.Equal(49.99M, game.Price.Value);
            Assert.Equal("USD", game.Price.Currency);
        }

        [Fact]
        public void CreateGame_ShouldSetPriceWithValidCurrency()
        {
            // Arrange
            var name = "Test Game";
            var genre = "Action";
            var price = 99.99M;
            var promotionId = (int?)null;
            var currency = "USD";

            // Act
            var game = new Game(name, genre, price, promotionId, currency);

            // Assert
            Assert.Equal(name, game.Name);
            Assert.Equal(genre, game.Genre);
            Assert.Equal(price, game.Price.Value);
            Assert.Equal(currency, game.Price.Currency);
        }

        [Fact]
        public void GetFinalPrice_ShouldReturnOriginalPrice_WhenNoPromotion()
        {
            var game = new Game("Game", "Action", 100M, null);

            var finalPrice = game.GetFinalPrice();

            Assert.Equal(100M, finalPrice);
        }

        [Fact]
        public void GetFinalPrice_ShouldReturnOriginalPrice_WhenPromotionIdIsNot4()
        {
            var game = new Game("Game", "Action", 100M, 1);

            var finalPrice = game.GetFinalPrice();

            Assert.Equal(100M, finalPrice);
        }

        [Fact]
        public void HasActivePromotion_ShouldReturnTrue_WhenHasPromotionId()
        {
            var game = new Game("Game", "Action", 100M, 4);

            var hasActive = game.HasActivePromotion();

            Assert.True(hasActive);
        }

        [Fact]
        public void HasActivePromotion_ShouldReturnFalse_WhenNoPromotionId()
        {
            var game = new Game("Game", "Action", 100M, null);

            var hasActive = game.HasActivePromotion();

            Assert.False(hasActive);
        }

        [Fact]
        public void RemovePromotion_ShouldClearPromotionData()
        {
            var game = new Game("Game", "Action", 100M, 4);
            var promotion = new Promotion(25M, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));
            game.Promotion = promotion;

            game.RemovePromotion();

            Assert.Null(game.PromotionId);
            Assert.Null(game.Promotion);
        }

        [Fact]
        public void GetDiscountAmount_ShouldReturnZero_WhenNoActivePromotion()
        {
            var game = new Game("Game", "Action", 100M, null);

            var discountAmount = game.GetDiscountAmount();

            Assert.Equal(0M, discountAmount);
        }

        [Fact]
        public void GetDiscountPercentage_ShouldReturnZero_WhenNoActivePromotion()
        {
            var game = new Game("Game", "Action", 100M, null);

            var percentage = game.GetDiscountPercentage();

            Assert.Equal(0M, percentage);
        }
    }
}

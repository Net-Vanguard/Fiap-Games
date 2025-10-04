using Moq;
using Xunit;
using Fiap.Infra.CrossCutting.Common.Elastic.Services;
using Fiap.Infra.CrossCutting.Common.Elastic.Models;
using Fiap.Infra.CrossCutting.Common.Http.CRM;

namespace Fiap.Unit.Tests._2._Application_Layer_Tests
{
    public class GamesServiceTests
    {
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<INotification> _mockNotification;
        private readonly Mock<ILogger<GamesService>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ICRMService> _mockCRMService;
        private readonly Mock<IElasticSearchService> _mockElasticService;
        private readonly Mock<IGameMongoRepository> _mockMongoRepository;
        private readonly Mock<IOutboxRepository> _mockOutboxRepository;
        private readonly GamesService _gameService;

        public GamesServiceTests()
        {
            _mockGameRepository = new Mock<IGameRepository>();
            _mockNotification = new Mock<INotification>();
            _mockLogger = new Mock<ILogger<GamesService>>();
            _mockElasticService = new Mock<IElasticSearchService>();
            _mockCRMService = new Mock<ICRMService>();
            _mockCacheService = new Mock<ICacheService>();
            _mockMongoRepository = new Mock<IGameMongoRepository>();
            _mockOutboxRepository = new Mock<IOutboxRepository>();

            _gameService = new GamesService(
                _mockNotification.Object,
                _mockGameRepository.Object,
                _mockElasticService.Object,
                _mockCacheService.Object,
                _mockCRMService.Object,
                _mockMongoRepository.Object,
                _mockOutboxRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateGame_ShouldReturnGameId_WhenValidRequest()
        {
            #region Arrange
            var request = new CreateGameRequest
            {
                Name = "Test Game",
                Genre = "Action",
                Price = 99.99M
            };

            // Setup ExistAsync to return false (game doesn't exist)
            _mockGameRepository
                .Setup(repo => repo.ExistAsync(It.IsAny<Expression<Func<Game, bool>>>()))
                .ReturnsAsync(false);

            _mockGameRepository
                .Setup(repo => repo.InsertOrUpdateAsync(It.IsAny<Game>()))
                .ReturnsAsync((Game game) =>
                {
                    game.Id = 1;
                    return game;
                });

            _mockGameRepository
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockOutboxRepository
                .Setup(repo => repo.InsertOrUpdateAsync(It.IsAny<OutboxMessage>()))
                .ReturnsAsync((OutboxMessage outbox) => outbox);

            _mockOutboxRepository
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockCacheService
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Setup Elasticsearch indexing to succeed
            _mockElasticService
                .Setup(x => x.IndexGamesAsync(It.IsAny<IEnumerable<GameDocument>>()))
                .ReturnsAsync(true);

            #endregion

            #region Act
            var result = await _gameService.CreateAsync(request);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Genre, result.Genre);
            Assert.Equal(request.Price, result.Price);

            // Verify cache invalidation
            _mockCacheService.Verify(
                x => x.RemoveAsync(EnumCacheTags.AllGames),
                Times.Once);

            // Verify outbox repository calls
            _mockOutboxRepository.Verify(
                x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>()),
                Times.Once);

            _mockOutboxRepository.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);

            // Verify Elasticsearch indexing was called (but may fail gracefully)
            _mockElasticService.Verify(
                x => x.IndexGamesAsync(It.IsAny<IEnumerable<GameDocument>>()),
                Times.Once);

            #endregion
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnGames_WhenGamesExist()
        {
            #region Arrange
            var games = new List<Game>
            {
                new Game("Game 1", "Action", 59.90M, null) { Id = 1 },
                new Game("Game 2", "Adventure", 49.90M, null) { Id = 2 }
            };

            // Setup cache miss - return null so it calls the fallback function
            _mockCacheService
                .Setup(x => x.GetOrSetAsync(EnumCacheTags.AllGames, It.IsAny<Func<Task<IEnumerable<Game>>>>()))
                .Returns<string, Func<Task<IEnumerable<Game>>>>((key, fallback) => fallback());

            _mockMongoRepository
                .Setup(repo => repo.GetAllWithPromotionsAsync())
                .ReturnsAsync(games);

            #endregion

            #region Act
            var result = await _gameService.GetAllAsync();
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, g => g.Id == 1 && g.Name == "Game 1");
            Assert.Contains(result, g => g.Id == 2 && g.Name == "Game 2");

            _mockCacheService.Verify(
                x => x.GetOrSetAsync(EnumCacheTags.AllGames, It.IsAny<Func<Task<IEnumerable<Game>>>>()), 
                Times.Once);

            _mockMongoRepository.Verify(repo => repo.GetAllWithPromotionsAsync(), Times.Once);
            #endregion
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoGamesExist()
        {
            #region Arrange
            var emptyGames = new List<Game>();

            _mockMongoRepository
                .Setup(repo => repo.GetAllWithPromotionsAsync())
                .ReturnsAsync(emptyGames);

            _mockCacheService
                .Setup(x => x.GetOrSetAsync(EnumCacheTags.AllGames, It.IsAny<Func<Task<IEnumerable<Game>>>>()))
                .Returns<string, Func<Task<IEnumerable<Game>>>>((key, fallback) => fallback());

            #endregion

            #region Act
            var result = await _gameService.GetAllAsync();
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _mockCacheService.Verify(
                x => x.GetOrSetAsync(EnumCacheTags.AllGames, It.IsAny<Func<Task<IEnumerable<Game>>>>()), 
                Times.Once);

            _mockMongoRepository.Verify(repo => repo.GetAllWithPromotionsAsync(), Times.Once);
            #endregion
        }

        [Fact]
        public async Task CreateGame_ShouldAddNotification_WhenGameAlreadyExists()
        {
            #region Arrange
            var request = new CreateGameRequest
            {
                Name = "Existing Game",
                Genre = "Action",
                Price = 49.99M
            };

            _mockGameRepository
                .Setup(repo => repo.ExistAsync(It.IsAny<Expression<Func<Game, bool>>>()))
                .ReturnsAsync(true);

            #endregion

            #region Act
            var result = await _gameService.CreateAsync(request);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            _mockNotification.Verify(
                n => n.AddNotification(
                    "Create Game",
                    $"The game '{request.Name}' has already been registered.",
                    NotificationModel.ENotificationType.BusinessRules),
                Times.Once
            );

            // Verify cache operations were NOT called when game already exists
            _mockCacheService.Verify(
                x => x.RemoveAsync(It.IsAny<string>()),
                Times.Never);

            // Verify outbox operations were NOT called when game already exists
            _mockOutboxRepository.Verify(
                x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>()),
                Times.Never);

            // Verify Elasticsearch was NOT called when game already exists
            _mockElasticService.Verify(
                x => x.IndexGamesAsync(It.IsAny<IEnumerable<GameDocument>>()),
                Times.Never);
            #endregion
        }

        [Fact]
        public async Task CreateGame_ShouldThrowException_WhenCurrencyIsInvalid()
        {
            #region Arrange
            var request = new CreateGameRequest
            {
                Name = "Invalid Currency Game",
                Genre = "Action",
                Price = 49.99M
            };

            _mockGameRepository
                .Setup(repo => repo.ExistAsync(It.IsAny<Expression<Func<Game, bool>>>()))
                .ReturnsAsync(false);

            _mockGameRepository
                .Setup(repo => repo.InsertOrUpdateAsync(It.IsAny<Game>()))
                .ThrowsAsync(new BusinessRulesException("Invalid currency: INVALID. Supported currencies are: USD, EUR, BRL, JPY, GBP"));

            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<BusinessRulesException>(() => _gameService.CreateAsync(request));

            // Verify cache operations were NOT called due to exception
            _mockCacheService.Verify(
                x => x.RemoveAsync(It.IsAny<string>()),
                Times.Never);

            // Verify outbox operations were NOT called due to exception
            _mockOutboxRepository.Verify(
                x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>()),
                Times.Never);

            // Verify Elasticsearch was NOT called due to exception
            _mockElasticService.Verify(
                x => x.IndexGamesAsync(It.IsAny<IEnumerable<GameDocument>>()),
                Times.Never);
            #endregion
        }

        [Fact]
        public async Task GetGameAsync_ShouldReturnGame_WhenGameExists()
        {
            // Arrange
            var gameId = 1;
            var game = new Game("Halo", "Shooter", 199.99M, null) { Id = gameId };

            // Simulate cache hit
            _mockCacheService
                .Setup(x => x.GetOrSetAsync(EnumCacheTags.GameId(gameId), It.IsAny<Func<Task<Game>>>()))
                .ReturnsAsync(game);

            // Act
            var result = await _gameService.GetAsync(gameId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(gameId, result.Id);
            Assert.Equal("Halo", result.Name);
            Assert.Equal("Shooter", result.Genre);
            Assert.Equal(199.99M, result.Price);

            _mockCacheService.Verify(
                x => x.GetOrSetAsync(EnumCacheTags.GameId(gameId), It.IsAny<Func<Task<Game>>>()), 
                Times.Once);

            // With cache hit, mongo repository should NOT be called
            _mockMongoRepository.Verify(
                repo => repo.GetByIdWithPromotionAsync(gameId),
                Times.Never);
        }

        [Fact]
        public async Task GetGameAsync_ShouldAddNotification_WhenGameDoesNotExist()
        {
            // Arrange
            var gameId = 99;

            _mockCacheService
                .Setup(x => x.GetOrSetAsync(EnumCacheTags.GameId(gameId), It.IsAny<Func<Task<Game>>>()))
                .Returns<string, Func<Task<Game>>>((key, fallback) => fallback());

            _mockMongoRepository
                .Setup(repo => repo.GetByIdWithPromotionAsync(gameId))
                .ReturnsAsync((Game?)null);

            // Act
            var result = await _gameService.GetAsync(gameId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            _mockNotification.Verify(
                n => n.AddNotification(
                    "Get game by id",
                    $"Game not found with id {gameId}",
                    NotificationModel.ENotificationType.NotFound),
                Times.Once
            );

            _mockCacheService.Verify(
                x => x.GetOrSetAsync(EnumCacheTags.GameId(gameId), It.IsAny<Func<Task<Game>>>()), 
                Times.Once);

            _mockMongoRepository.Verify(
                repo => repo.GetByIdWithPromotionAsync(gameId),
                Times.Once);
        }

        // [Fact]
        // public async Task SyncSeedDataAsync_ValidData_CompletesSuccessfully()
        // {
        //     // Arrange
        //     var games = new List<Game>
        //     {
        //         new() { Id = 1, Name = "Game1", Genre = "Action", Price = new Money(50, "BRL") },
        //         new() { Id = 2, Name = "Game2", Genre = "RPG", Price = new Money(60, "BRL") }
        //     };
        //
        //     _mockGameRepository.Setup(x => x.GetAllWithPromotionsAsync()).ReturnsAsync(games);
        //     _mockMongoRepository.Setup(x => x.GetByIdWithPromotionAsync(It.IsAny<int>())).ReturnsAsync((Game)null);
        //     _mockElasticService.Setup(x => x.IndexGamesAsync(It.IsAny<IEnumerable<GameDocument>>())).ReturnsAsync(true);
        //
        //     // Act
        //     await _gameService.SyncSeedDataAsync();
        //
        //     // Assert
        //     _mockGameRepository.Verify(x => x.GetAllWithPromotionsAsync(), Times.Once);
        //     _mockElasticService.Verify(x => x.IndexGamesAsync(It.IsAny<IEnumerable<GameDocument>>()), Times.Once);
        // }
    }
}
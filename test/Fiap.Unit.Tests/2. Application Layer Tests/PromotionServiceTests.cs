namespace Fiap.Unit.Tests._2._Application_Layer_Tests
{
    public class PromotionServiceTests
    {
        private readonly Mock<IPromotionRepository> _mockPromotionRepositoryMock;
        private readonly Mock<IGameRepository> _mockGameRepositoryMock;
        private readonly Mock<INotification> _mockNotification;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<PromotionsService>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
		private readonly Mock<IPromotionMongoRepository> _mockPromotionMongoRepository;
		private readonly Mock<IOutboxRepository> _mockOutboxRepository;
		private readonly PromotionsService _promotionService;

        public PromotionServiceTests()
        {
            _mockPromotionRepositoryMock = new Mock<IPromotionRepository>();
            _mockGameRepositoryMock = new Mock<IGameRepository>();
            _mockNotification = new Mock<INotification>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<PromotionsService>>();
            _mockCacheService = new Mock<ICacheService>();
			_mockPromotionMongoRepository = new Mock<IPromotionMongoRepository>();
			_mockOutboxRepository = new Mock<IOutboxRepository>();
            _promotionService = new PromotionsService(
                _mockNotification.Object,
                _mockPromotionRepositoryMock.Object,
                _mockGameRepositoryMock.Object,
                _mockUnitOfWork.Object,
                _mockCacheService.Object,
                _mockLogger.Object,
                _mockPromotionMongoRepository.Object,
                _mockOutboxRepository.Object
			);
        }

        [Fact]
        public async Task CreatePromotion_ShouldReturnPromotionId_WhenValidRequest()
        {
            #region Arrange
            var now = DateTime.UtcNow;

            var request = new CreatePromotionRequest
            {
                Discount = 10,
                ExpirationDate = now.AddDays(30),
                GameId = [1, 2, 3]
            };

            _mockPromotionRepositoryMock
                .Setup(repo => repo.InsertOrUpdateAsync(It.IsAny<Promotion>()))
                .ReturnsAsync((Promotion p) =>
                {
                    p.Id = 1;
                    return p;
                });

            _mockPromotionRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockGameRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), true))
                .ReturnsAsync(new Game());

            _mockGameRepositoryMock
                .Setup(repo => repo.UpdateRangeAsync(It.IsAny<IEnumerable<Game>>()))
                .Returns(Task.CompletedTask);

            _mockGameRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockCacheService
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            #endregion

            #region Act
            var result = await _promotionService.CreateAsync(request);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.True(result.PromotionId > 0);
            Assert.Equal(request.Discount, result.Discount);
            Assert.Equal(request.ExpirationDate, result.EndDate);
            Assert.True(
                (DateTime.UtcNow - result.StartDate).TotalSeconds < 5,
                $"StartDate was too far off from expected time: {result.StartDate}"
            );

            _mockCacheService.Verify(
                x => x.RemoveAsync(EnumCacheTags.AllPromotions),
                Times.Once);

            #endregion
        }

        [Fact]
        public async Task UpdatePromotion_ShouldReturnTrue_WhenValidRequest()
        {
            #region Arrange
            int promotionId = 1;
            var request = new UpdatePromotionRequest
            {
                Discount = 10,
                ExpirationDate = DateTime.UtcNow.AddDays(30)
            };

            var promotion = new Promotion(request.Discount ?? 0, DateTime.UtcNow, request.ExpirationDate ?? DateTime.UtcNow)
            {
                Id = promotionId
            };

            _mockPromotionRepositoryMock.Setup(repo => repo.GetByIdAsync(promotionId, true)) 
                .ReturnsAsync(promotion);

            _mockPromotionRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Promotion>()))
                .Returns(Task.CompletedTask);

            _mockPromotionRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockGameRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockCacheService
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            #endregion

            #region Act
            var result = await _promotionService.UpdateAsync(promotionId, request);
            #endregion

            #region Assert
            Assert.NotNull(result);

            _mockCacheService.Verify(
                x => x.RemoveAsync(EnumCacheTags.AllPromotions),
                Times.Once);

            _mockCacheService.Verify(
                x => x.RemoveAsync(EnumCacheTags.PromotionId(promotionId)),
                Times.Once);
            #endregion
        }


        [Fact]
        public async Task UpdatePromotion_ShouldAddNotification_WhenPromotionNotFound()
        {
            #region Arrange
            var promotionId = 1;
            var request = new UpdatePromotionRequest
            {
                Discount = 15,
                ExpirationDate = DateTime.UtcNow.AddDays(10)
            };

            _mockPromotionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(promotionId, true))
                .ReturnsAsync((Promotion?)null);
            #endregion

            #region Act
            var result = await _promotionService.UpdateAsync(promotionId, request);
            #endregion

            #region Assert
            Assert.Null(result);
            _mockNotification.Verify(n =>
                n.AddNotification("PromotionId", "Promotion not found", NotificationModel.ENotificationType.NotFound),
                Times.Once);

            _mockCacheService.Verify(
                x => x.RemoveAsync(It.IsAny<string>()),
                Times.Never);
            #endregion
        }

        [Fact]
        public async Task UpdatePromotion_ShouldAddNotification_WhenSomeGamesNotFound()
        {
            #region Arrange
            int promotionId = 1;

            var request = new UpdatePromotionRequest
            {
                Discount = 15,
                ExpirationDate = DateTime.UtcNow.AddDays(20),
                GameId = [101, 102, 999]
            };

            var promotion = new Promotion(request.Discount.Value, DateTime.UtcNow, request.ExpirationDate.Value)
            {
                Id = promotionId
            };

            var game1 = new Game() { Id = 101, Name = "Game 1", Genre = "Action", Price = new Money(59.90M, "BRL") };
            var game2 = new Game() { Id = 102, Name = "Game 2", Genre = "Adventure", Price = new Money(49.90M, "BRL") };

            _mockPromotionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(promotionId, true)) 
                .ReturnsAsync(promotion);

            _mockGameRepositoryMock
                .Setup(repo => repo.GetByIdAsync(101, false)) 
                .ReturnsAsync(game1);

            _mockGameRepositoryMock
                .Setup(repo => repo.GetByIdAsync(102, false)) 
                .ReturnsAsync(game2);

            _mockGameRepositoryMock
                .Setup(repo => repo.GetByIdAsync(999, false)) 
                .ReturnsAsync((Game?)null);

            _mockPromotionRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Promotion>()))
                .Returns(Task.CompletedTask);

            _mockPromotionRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockGameRepositoryMock
                .Setup(repo => repo.UpdateRangeAsync(It.IsAny<IEnumerable<Game>>()))
                .Returns(Task.CompletedTask);

            _mockGameRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockCacheService
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            #endregion

            #region Act
            var result = await _promotionService.UpdateAsync(promotionId, request);
            #endregion

            #region Assert
            Assert.NotNull(result);

            _mockNotification.Verify(n =>
                n.AddNotification("Game with ID 999 Not found", "Not Found", ENotificationType.NotFound),
                Times.Once);

            _mockCacheService.Verify(
                x => x.RemoveAsync(EnumCacheTags.AllPromotions),
                Times.Once);

            _mockCacheService.Verify(
                x => x.RemoveAsync(EnumCacheTags.PromotionId(promotionId)),
                Times.Once);
            #endregion
        }

        [Fact]
        public async Task CreatePromotion_ShouldAddNotification_WhenSomeGamesNotFound()
        {
            #region Arrange
            var request = new CreatePromotionRequest
            {
                Discount = 20,
                ExpirationDate = DateTime.UtcNow.AddDays(10),
                GameId = [1, 2, 3]
            };

            _mockPromotionRepositoryMock
                .Setup(repo => repo.InsertOrUpdateAsync(It.IsAny<Promotion>()))
                .ReturnsAsync((Promotion p) =>
                {
                    p.Id = 99;
                    return p;
                });

            _mockPromotionRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockGameRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), true))
                .ReturnsAsync((Game?)null);

            _mockGameRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockCacheService
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            #endregion

            #region Act
            var result = await _promotionService.CreateAsync(request);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(99, result.PromotionId);
            _mockNotification.Verify(n =>
                n.AddNotification(It.Is<string>(s => s.Contains("Game with ID")), "Not Found", NotificationModel.ENotificationType.NotFound),
                Times.Exactly(3));

            _mockCacheService.Verify(
                x => x.RemoveAsync(EnumCacheTags.AllPromotions),
                Times.Once);
            #endregion
        }

        [Fact]
        public async Task GetPromotionAsync_ShouldReturnPromotion_WhenItExists()
        {
            // Arrange
            var promotionId = 1;
            var now = DateTime.UtcNow;
            var expiration = now.AddDays(10);

            var promotion = new Promotion(10, now, expiration)
            {
                Id = promotionId
            };

            // Simulate cache hit
            _mockCacheService
                .Setup(x => x.GetOrSetAsync(EnumCacheTags.PromotionId(promotionId), It.IsAny<Func<Task<Promotion>>>()))
                .ReturnsAsync(promotion);

            _mockPromotionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(promotionId, true)) 
                .ReturnsAsync(promotion);

            // Act
            var result = await _promotionService.GetPromotionAsync(promotionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(promotionId, result.PromotionId);
            Assert.Equal(promotion.Discount.Value, result.Discount);
            Assert.Equal(promotion.StartDate.Value.Date, result.StartDate.Date);
            Assert.Equal(promotion.EndDate.Value.Date, result.EndDate.Date);

            _mockCacheService.Verify(
                x => x.GetOrSetAsync(EnumCacheTags.PromotionId(promotionId), It.IsAny<Func<Task<Promotion>>>()), 
                Times.Once);

            _mockPromotionRepositoryMock.Verify(
                repo => repo.GetByIdAsync(promotionId, true),
                Times.Never);
        }

        [Fact]
        public async Task GetPromotionAsync_ShouldAddNotification_WhenPromotionNotFound()
        {
            // Arrange
            var promotionId = 999;

            // Simulate cache miss that calls repository
            _mockCacheService
                .Setup(x => x.GetOrSetAsync(EnumCacheTags.PromotionId(promotionId), It.IsAny<Func<Task<Promotion>>>()))
                .Returns<string, Func<Task<Promotion>>>((key, fallback) => fallback());

			//_mockPromotionMongoRepository
			//	.Setup(repo => repo.GetByIdAsync(promotionId)) 
			//             .ReturnsAsync((Promotion?)null);

			_mockPromotionMongoRepository
				.Setup(repo => repo.GetByIdAsync(promotionId))
				.ReturnsAsync((Promotion?)null);

			// Act
			var result = await _promotionService.GetPromotionAsync(promotionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.PromotionId);
            _mockNotification.Verify(n =>
                n.AddNotification("Promotion not found", $"Promotion with id {promotionId} not found", ENotificationType.NotFound),
                Times.Once);

            _mockCacheService.Verify(
                x => x.GetOrSetAsync(EnumCacheTags.PromotionId(promotionId), It.IsAny<Func<Task<Promotion>>>()), 
                Times.Once);

			_mockPromotionMongoRepository.Verify(
				repo => repo.GetByIdAsync(promotionId),
				Times.Once);
		}
    }
}

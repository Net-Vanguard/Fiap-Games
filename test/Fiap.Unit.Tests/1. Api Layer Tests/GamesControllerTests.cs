namespace Fiap.Unit.Tests._1._Api_Layer_Tests
{
    public class GamesControllerTests
    {
        readonly Mock<IGamesService> _gamesServiceMock;
        readonly Mock<INotification> _notificationMock;
        readonly GamesController _controller;

        public GamesControllerTests()
        {
            _gamesServiceMock = new Mock<IGamesService>();
            _notificationMock = new Mock<INotification>();
            _controller = new GamesController(_gamesServiceMock.Object, _notificationMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    RouteData = new Microsoft.AspNetCore.Routing.RouteData()
                }
            };
            _controller.ControllerContext.RouteData.Values["controller"] = "Games";
            _controller.ControllerContext.RouteData.Values["version"] = "1.0";
        }

        #region CreateGame

        [Fact]
        public async Task CreateGame_ShouldReturnCreated_WhenServiceReturnsSuccess()
        {
            #region Arrange
            var request = new CreateGameRequest
            {
                Name = "Test Game",
                Genre = "Action",
                Price = 59.99M
            };

            var response = new GameResponse
            {
                Id = 1,
                Name = request.Name,
                Genre = request.Genre,
                Price = request.Price,
                PromotionId = null,
                PriceWithDiscount = request.Price
            };

            _gamesServiceMock
                .Setup(x => x.CreateAsync(request))
                .ReturnsAsync(response);
            #endregion

            #region Act
            var result = await _controller.Create(request);
            #endregion

            #region Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            var anonymousResponse = createdResult.Value;
            
            // Verificar propriedades do tipo anônimo
            var successProp = anonymousResponse.GetType().GetProperty("success");
            var dataProp = anonymousResponse.GetType().GetProperty("data");
            
            Assert.True((bool)successProp.GetValue(anonymousResponse));
            
            var gameData = (GameResponse)dataProp.GetValue(anonymousResponse);
            Assert.Equal(response.Name, gameData.Name);
            #endregion
        }

        #endregion

        #region GetAllGames

        [Fact]
        public async Task GetAllGames_ShouldReturnOk_WhenServiceReturnsGames()
        {
            #region Arrange
            var mockList = new List<GameResponse>
            {
                new GameResponse { Id = 1, Name = "Game 1", Genre = "Action", Price = 10, PriceWithDiscount = 10 },
                new GameResponse { Id = 2, Name = "Game 2", Genre = "RPG", Price = 20, PriceWithDiscount = 20 }
            };

            _gamesServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(mockList);
            #endregion

            #region Act
            var result = await _controller.GetAll();
            #endregion

            #region Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<IEnumerable<GameResponse>>>(okResult.Value);

            Assert.True(baseResponse.Success);
            Assert.Equal(2, baseResponse.Data.Count());
            #endregion
        }

        [Fact]
        public async Task GetAllGames_ShouldReturnEmptyList_WhenElasticsearchReturnsNoGames()
        {
            #region Arrange
            var emptyList = new List<GameResponse>();

            _gamesServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(emptyList);
            #endregion

            #region Act
            var result = await _controller.GetAll();
            #endregion

            #region Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<IEnumerable<GameResponse>>>(okResult.Value);

            Assert.True(baseResponse.Success);
            Assert.Empty(baseResponse.Data);
            #endregion
        }

        #endregion

        #region GetGame

        [Fact]
        public async Task GetGame_ShouldReturnOk_WhenGameExists()
        {
            // Arrange
            var gameId = 1;
            var response = new GameResponse
            {
                Id = gameId,
                Name = "Zelda",
                Genre = "Adventure",
                Price = 199.90M,
                PriceWithDiscount = 199.90M
            };

            _gamesServiceMock
                .Setup(x => x.GetAsync(gameId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAsync(gameId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<GameResponse>>(okResult.Value);
            Assert.True(baseResponse.Success);
            Assert.Equal(response.Id, baseResponse.Data.Id);
        }

        #endregion

        #region Elasticsearch Endpoints

        [Fact]
        public async Task GetMostPopularGames_ShouldReturnOk_WhenServiceReturnsPopularGames()
        {
            #region Arrange
            var popularGames = new List<GameResponse>
            {
                new GameResponse { Id = 1, Name = "Popular Game 1", Genre = "Action", Price = 59.99M, PriceWithDiscount = 59.99M },
                new GameResponse { Id = 2, Name = "Popular Game 2", Genre = "Adventure", Price = 49.99M, PriceWithDiscount = 39.99M }
            };

            _gamesServiceMock
                .Setup(x => x.GetMostPopularGamesFromElasticsearchAsync())
                .ReturnsAsync(popularGames);
            #endregion

            #region Act
            var result = await _controller.GetMostPopularGames();
            #endregion

            #region Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var anonymousResponse = okResult.Value;
            
            var successProp = anonymousResponse.GetType().GetProperty("success");
            var dataProp = anonymousResponse.GetType().GetProperty("data");
            
            Assert.True((bool)successProp.GetValue(anonymousResponse));
            
            var games = (IEnumerable<GameResponse>)dataProp.GetValue(anonymousResponse);
            Assert.Equal(2, games.Count());
            Assert.Contains(games, g => g.Name == "Popular Game 1");
            #endregion
        }

        [Fact]
        public async Task GetMostPopularGames_ShouldReturnEmptyList_WhenNoPopularGamesFound()
        {
            #region Arrange
            var emptyList = new List<GameResponse>();

            _gamesServiceMock
                .Setup(x => x.GetMostPopularGamesFromElasticsearchAsync())
                .ReturnsAsync(emptyList);
            #endregion

            #region Act
            var result = await _controller.GetMostPopularGames();
            #endregion

            #region Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var anonymousResponse = okResult.Value;
            
            var successProp = anonymousResponse.GetType().GetProperty("success");
            var dataProp = anonymousResponse.GetType().GetProperty("data");
            
            Assert.True((bool)successProp.GetValue(anonymousResponse));
            
            var games = (IEnumerable<GameResponse>)dataProp.GetValue(anonymousResponse);
            Assert.Empty(games);
            #endregion
        }

        [Fact]
        public async Task GetUserRecommendations_ShouldReturnOk_WhenServiceReturnsRecommendations()
        {
            #region Arrange
            var userId = 123;
            var recommendations = new List<GameResponse>
            {
                new GameResponse { Id = 3, Name = "Recommended Game 1", Genre = "Action", Price = 39.99M, PriceWithDiscount = 39.99M },
                new GameResponse { Id = 4, Name = "Recommended Game 2", Genre = "Adventure", Price = 29.99M, PriceWithDiscount = 29.99M }
            };

            _gamesServiceMock
                .Setup(x => x.GetUserRecommendationsAsync(userId))
                .ReturnsAsync(recommendations);
            #endregion

            #region Act
            var result = await _controller.GetUserRecommendations(userId);
            #endregion

            #region Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var anonymousResponse = okResult.Value;
            
            var successProp = anonymousResponse.GetType().GetProperty("success");
            var dataProp = anonymousResponse.GetType().GetProperty("data");
            
            Assert.True((bool)successProp.GetValue(anonymousResponse));
            
            var games = (IEnumerable<GameResponse>)dataProp.GetValue(anonymousResponse);
            Assert.Equal(2, games.Count());
            Assert.Contains(games, g => g.Name == "Recommended Game 1");
            #endregion
        }

        [Fact]
        public async Task GetUserRecommendations_ShouldReturnEmptyList_WhenNoRecommendationsFound()
        {
            #region Arrange
            var userId = 999;
            var emptyList = new List<GameResponse>();

            _gamesServiceMock
                .Setup(x => x.GetUserRecommendationsAsync(userId))
                .ReturnsAsync(emptyList);
            #endregion

            #region Act
            var result = await _controller.GetUserRecommendations(userId);
            #endregion

            #region Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var anonymousResponse = okResult.Value;
            
            var successProp = anonymousResponse.GetType().GetProperty("success");
            var dataProp = anonymousResponse.GetType().GetProperty("data");
            
            Assert.True((bool)successProp.GetValue(anonymousResponse));
            
            var games = (IEnumerable<GameResponse>)dataProp.GetValue(anonymousResponse);
            Assert.Empty(games);
            #endregion
        }

        #endregion

        // #region SyncSeedData Tests - Métodos removidos
        //
        // [Fact]
        // public async Task SyncSeedData_ValidData_ReturnsSuccess()
        // {
        //     // Arrange
        //     var expectedResponse = new { message = "Seed synchronization completed successfully" };
        //     _gamesServiceMock
        //         .Setup(x => x.SyncSeedDataAsync())
        //         .ReturnsAsync(expectedResponse);
        //
        //     // Act
        //     var result = await _controller.SyncSeedData();
        //
        //     // Assert
        //     var okResult = Assert.IsType<OkObjectResult>(result);
        //     Assert.NotNull(okResult.Value);
        //
        //     _gamesServiceMock.Verify(x => x.SyncSeedDataAsync(), Times.Once);
        // }
        //
        // [Fact]
        // public async Task SyncSeedData_ServiceThrowsException_ReturnsInternalServerError()
        // {
        //     // Arrange
        //     _gamesServiceMock
        //         .Setup(x => x.SyncSeedDataAsync())
        //         .ThrowsAsync(new Exception("Sync error"));
        //
        //     // Act & Assert
        //     await Assert.ThrowsAsync<Exception>(() => _controller.SyncSeedData());
        // }
        //
        // #endregion

        #region Popular Games Tests - Diagnóstico do array vazio

        [Fact]
        public async Task GetMostPopularGames_ShouldReturnEmptyArray_WhenElasticsearchNotIndexed()
        {
            #region Arrange
            // Simula o cenário atual: Elasticsearch não tem jogos indexados
            var emptyList = new List<GameResponse>();

            _gamesServiceMock
                .Setup(x => x.GetMostPopularGamesFromElasticsearchAsync())
                .ReturnsAsync(emptyList);

            // No notifications (this is not an error, just no data)
            _notificationMock
                .Setup(x => x.HasNotification)
                .Returns(false);
            #endregion

            #region Act
            var result = await _controller.GetMostPopularGames();
            #endregion

            #region Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var anonymousResponse = okResult.Value;
            
            var successProp = anonymousResponse.GetType().GetProperty("success");
            var dataProp = anonymousResponse.GetType().GetProperty("data");
            
            // Should still return success = true, but with empty data
            Assert.True((bool)successProp.GetValue(anonymousResponse));
            
            var games = (IEnumerable<GameResponse>)dataProp.GetValue(anonymousResponse);
            Assert.Empty(games);
            
            // Verify service was called
            _gamesServiceMock.Verify(x => x.GetMostPopularGamesFromElasticsearchAsync(), Times.Once);
            #endregion
        }

        #endregion

        #region Service Error Scenarios

        [Fact]
        public async Task GetMostPopularGames_ShouldHandleServiceException()
        {
            #region Arrange
            _gamesServiceMock
                .Setup(x => x.GetMostPopularGamesFromElasticsearchAsync())
                .ThrowsAsync(new Exception("Elasticsearch connection failed"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetMostPopularGames());
            #endregion
        }

        [Fact]
        public async Task GetUserRecommendations_ShouldHandleServiceException()
        {
            #region Arrange
            var userId = 123;
            _gamesServiceMock
                .Setup(x => x.GetUserRecommendationsAsync(userId))
                .ThrowsAsync(new Exception("CRM service unavailable"));
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetUserRecommendations(userId));
            #endregion
        }

        // [Fact]
        // public async Task SyncSeedData_ShouldHandleServiceException()
        // {
        //     #region Arrange
        //     _gamesServiceMock
        //         .Setup(x => x.SyncSeedDataAsync())
        //         .ThrowsAsync(new Exception("Database connection failed"));
        //     #endregion
        //
        //     #region Act & Assert
        //     await Assert.ThrowsAsync<Exception>(() => _controller.SyncSeedData());
        //     #endregion
        // }

        #endregion
    }
}

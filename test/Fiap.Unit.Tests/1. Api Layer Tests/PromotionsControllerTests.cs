

namespace Fiap.Unit.Tests._1._Api_Layer_Tests
{
    public class PromotionsControllerTests
    {
        readonly Mock<IPromotionsService> _promotionsServiceMock;
        readonly Mock<INotification> _mockNotification;
        readonly PromotionsController _controller;

        public PromotionsControllerTests()
        {
            _promotionsServiceMock = new Mock<IPromotionsService>();
            _mockNotification = new Mock<INotification>();
            _controller = new PromotionsController(_promotionsServiceMock.Object, _mockNotification.Object)
            {
                ControllerContext = new ControllerContext
                {
                    RouteData = new Microsoft.AspNetCore.Routing.RouteData()
                }
            };
            _controller.ControllerContext.RouteData.Values["controller"] = "Promotions";
            _controller.ControllerContext.RouteData.Values["version"] = "1.0";
        }

        #region CreatePromotion

        [Fact]
        public async Task CreatePromotion_ShouldReturnOk_WhenServiceReturnsSuccess()
        {
            #region Arrange
            var mockCreateRequest = new CreatePromotionRequest
            {
                Discount = 10,
                ExpirationDate = DateTime.UtcNow.AddDays(30),
                GameId = [1, 2, 3]
            };

            var mockCreateResponse = new PromotionResponse
            {
                PromotionId = 1,
                Discount = mockCreateRequest.Discount,
                StartDate = DateTime.UtcNow,
                EndDate = mockCreateRequest.ExpirationDate
            };
            #endregion

            #region Act
            _promotionsServiceMock
                .Setup(x => x.CreateAsync(mockCreateRequest))
                .ReturnsAsync(mockCreateResponse);

            var result = await _controller.CreateAsync(mockCreateRequest);
            #endregion

            #region Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            var json = JsonSerializer.Serialize(createdResult.Value);
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            Assert.True(root.GetProperty("success").GetBoolean());
            var data = root.GetProperty("data");

            Assert.Equal(mockCreateResponse.PromotionId, data.GetProperty("PromotionId").GetInt32());
            Assert.Equal(mockCreateResponse.Discount, data.GetProperty("Discount").GetInt32());

            var parsedStartDate = data.GetProperty("StartDate").GetDateTime();
            var parsedEndDate = data.GetProperty("EndDate").GetDateTime();

            Assert.Equal(mockCreateResponse.StartDate.Date, parsedStartDate.Date);
            Assert.Equal(mockCreateResponse.EndDate.Date, parsedEndDate.Date);

            #endregion
        }
        #endregion

        #region UpdatePromotion

        [Fact]
        public async Task UpdatePromotion_ShouldReturnOk_WhenServiceReturnsSuccess()
        {
            #region Arrange
            int promotionId = 1;

            var mockUpdateRequest = new UpdatePromotionRequest
            {
                Discount = 20,
                ExpirationDate = DateTime.UtcNow.AddDays(60),
                GameId = [1, 2, 3]
            };
            var mockUpdateResponse = new PromotionResponse
            {
                PromotionId = 1,
                Discount = mockUpdateRequest.Discount.Value,
                StartDate = DateTime.UtcNow,
                EndDate = mockUpdateRequest.ExpirationDate.Value
            };
            #endregion

            #region Act
            _promotionsServiceMock
                .Setup(x => x.UpdateAsync(promotionId, mockUpdateRequest));

            var result = await _controller.UpdateAsync(promotionId, mockUpdateRequest);
            #endregion

            #region Assert
            Assert.IsType<NoContentResult>(result);
            #endregion
        }
        #endregion

        [Fact]
        public async Task GetPromotion_ShouldReturnOk_WhenServiceReturnsPromotion()
        {
            // Arrange
            int promotionId = 1;
            var promotionResponse = new PromotionResponse
            {
                PromotionId = promotionId,
                Discount = 15,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7)
            };

            _promotionsServiceMock
                .Setup(x => x.GetPromotionAsync(promotionId))
                .ReturnsAsync(promotionResponse);

            // Act
            var result = await _controller.GetAsync(promotionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<PromotionResponse>>(okResult.Value);
            Assert.True(baseResponse.Success);
            Assert.Equal(promotionResponse.PromotionId, baseResponse.Data.PromotionId);
        }

    }
}

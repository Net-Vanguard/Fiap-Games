namespace Fiap.Unit.Tests._3._Domain_Layer_Tests.Entities
{
    public class PromotionTest
    {
        [Fact]
        public void PromotionSuccess()
        {
            #region Arrange
            var discount = 10.0M;
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(30);
            #endregion

            #region Act
            var promotion = new Promotion(discount, startDate, endDate);
            #endregion

            #region Assert
            Assert.Equal(discount, promotion.Discount.Value);
            Assert.Equal(startDate, promotion.StartDate);
            Assert.Equal(endDate, promotion.EndDate);
            #endregion
        }

        [Fact]
        public void Promotion_IsExpired_ReturnsTrue_WhenDateHasPassed()
        {
            #region Arrange
            var promotion = new Promotion(10.0M, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1));
            #endregion

            #region Act
            var isExpired = promotion.IsExpired();
            #endregion

            #region Assert
            Assert.True(isExpired);
            #endregion
        }

        [Fact]
        public void Promotion_IsActive_ReturnsTrue_WhenWithinPeriod()
        {
            #region Arrange
            var promotion = new Promotion(10.0M, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10));
            #endregion

            #region Act
            var isActive = promotion.IsActive();
            #endregion

            #region Assert
            Assert.True(isActive);
            #endregion
        }

        [Fact]
        public void ValidatePeriod_ShouldThrow_WhenEndDateIsBeforeStartDate()
        {
            #region Arrange
            var promotion = new Promotion(10, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1));
            #endregion

            #region Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() => promotion.ValidatePeriod());

            Assert.Equal("Promotion end date cannot be earlier than the start date.", ex.Message);
            #endregion
        }

        [Fact]
        public void GetDiscountedPrice_ReturnsCorrectValue()
        {
            #region Arrange
            var promotion = new Promotion(25.0M, DateTime.UtcNow, DateTime.UtcNow.AddDays(5));
            var originalPrice = 100.0M;
            #endregion

            #region Act
            var discountedPrice = promotion.GetDiscountedPrice(originalPrice);
            #endregion

            #region Assert
            Assert.Equal(75.0M, discountedPrice);
            #endregion
        }

        [Fact]
        public void UpdateDiscount_UpdatesValuesCorrectly()
        {
            #region Arrange
            var promotion = new Promotion(10.0M, DateTime.UtcNow, DateTime.UtcNow.AddDays(5));
            var newDiscount = 20.0M;
            var newEndDate = DateTime.UtcNow.AddDays(10);
            #endregion

            #region Act
            promotion.UpdateDiscount(newDiscount, newEndDate);
            #endregion

            #region Assert
            Assert.Equal(newDiscount, promotion.Discount.Value);
            Assert.Equal(newEndDate, promotion.EndDate);
            #endregion
        }
    }
}

using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using System;
using Xunit;

namespace CourtBooking.Test.Domain.Models
{
    public class CourtPromotionTests
    {
        [Fact]
        public void Create_Should_ReturnValidCourtPromotion_WhenParametersAreValid()
        {
            // Arrange
            var courtId = CourtId.Of(Guid.NewGuid());
            var description = "Summer Promotion";
            var discountType = "Percentage";
            var discountValue = 25.0m;
            var validFrom = DateTime.Today;
            var validTo = DateTime.Today.AddDays(30);

            // Act
            var promotion = CourtPromotion.Create(courtId, description, discountType, discountValue, validFrom, validTo);

            // Assert
            Assert.NotNull(promotion);
            Assert.NotEqual(Guid.Empty, promotion.Id.Value);
            Assert.Equal(courtId, promotion.CourtId);
            Assert.Equal(description, promotion.Description);
            Assert.Equal(discountType, promotion.DiscountType);
            Assert.Equal(discountValue, promotion.DiscountValue);
            Assert.Equal(validFrom, promotion.ValidFrom);
            Assert.Equal(validTo, promotion.ValidTo);
            Assert.True(promotion.CreatedAt > DateTime.MinValue);
            Assert.Null(promotion.LastModified);
        }

        [Fact]
        public void Update_Should_UpdatePromotionProperties_WhenParametersAreValid()
        {
            // Arrange
            var promotion = CreateValidCourtPromotion();
            var newDescription = "Updated Promotion";
            var newDiscountType = "FixedAmount";
            var newDiscountValue = 50.0m;
            var newStartDate = DateTime.Today.AddDays(1);
            var newEndDate = DateTime.Today.AddDays(30);

            // Act
            promotion.Update(newDescription, newDiscountType, newDiscountValue, newStartDate, newEndDate);

            // Assert
            Assert.Equal(newDescription, promotion.Description);
            Assert.Equal(newDiscountType, promotion.DiscountType);
            Assert.Equal(newDiscountValue, promotion.DiscountValue);
            Assert.Equal(newStartDate, promotion.ValidFrom);
            Assert.Equal(newEndDate, promotion.ValidTo);
            Assert.NotNull(promotion.LastModified);
        }

        [Fact]
        public void Create_Should_SetCreatedAtToCurrentDateTime()
        {
            // Arrange
            var before = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var promotion = CreateValidCourtPromotion();
            var after = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.True(promotion.CreatedAt >= before);
            Assert.True(promotion.CreatedAt <= after);
        }

        private CourtPromotion CreateValidCourtPromotion()
        {
            return CourtPromotion.Create(
                CourtId.Of(Guid.NewGuid()),
                "Test Promotion",
                "Percentage",
                15.0m,
                DateTime.Today,
                DateTime.Today.AddDays(15)
            );
        }
    }
}
using CourtBooking.Application.CourtManagement.Queries.GetCourtPromotions;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetCourtPromotionsQueryTests
    {
        [Fact]
        public void Constructor_Should_SetCourtId_When_Called()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var query = new GetCourtPromotionsQuery(courtId, userId, "User");

            // Assert
            Assert.Equal(courtId, query.CourtId);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentException_When_CourtIdIsEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new GetCourtPromotionsQuery(Guid.Empty, userId, "User"));

            Assert.Contains("rỗng", exception.Message);
        }
    }
}
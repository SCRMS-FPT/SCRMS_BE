using CourtBooking.Application.CourtManagement.Queries.GetSportCenterById;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetSportCenterByIdQueryTests
    {
        [Fact]
        public void Constructor_Should_SetSportCenterId_When_Called()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();

            // Act
            var query = new GetSportCenterByIdQuery(sportCenterId);

            // Assert
            Assert.Equal(sportCenterId, query.Id);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentException_When_SportCenterIdIsEmpty()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => new GetSportCenterByIdQuery(Guid.Empty)
            );

            Assert.Contains("rỗng", exception.Message);
        }
    }
}
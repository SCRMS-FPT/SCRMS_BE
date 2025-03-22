using CourtBooking.Application.CourtManagement.Queries.GetAllCourtsOfSportCenter;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetAllCourtsOfSportCenterQueryTests
    {
        [Fact]
        public void Constructor_Should_SetSportCenterId_When_Called()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();

            // Act
            var query = new GetAllCourtsOfSportCenterQuery(sportCenterId);

            // Assert
            Assert.Equal(sportCenterId, query.SportCenterId);
        }

        [Fact]
        public void Constructor_Should_AcceptEmptyGuid_When_Called()
        {
            // Arrange & Act
            var query = new GetAllCourtsOfSportCenterQuery(Guid.Empty);

            // Assert
            Assert.Equal(Guid.Empty, query.SportCenterId);
        }
    }
}
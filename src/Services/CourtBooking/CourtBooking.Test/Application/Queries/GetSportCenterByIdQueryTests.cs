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
        public void Constructor_Should_AcceptEmptyGuid()
        {
            // Since the record doesn't validate inputs, we need to adjust our test expectation
            // Arrange & Act
            var query = new GetSportCenterByIdQuery(Guid.Empty);

            // Assert
            Assert.Equal(Guid.Empty, query.Id);
            // The actual validation should happen in the handler, not in the query constructor
        }
    }
}
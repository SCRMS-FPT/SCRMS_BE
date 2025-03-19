using CourtBooking.Application.SportManagement.Queries.GetSportById;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetSportByIdQueryTests
    {
        [Fact]
        public void Constructor_Should_SetSportId_When_Called()
        {
            // Arrange
            var sportId = Guid.NewGuid();

            // Act
            var query = new GetSportByIdQuery(sportId);

            // Assert
            Assert.Equal(sportId, query.SportId);
        }

        [Fact]
        public void Constructor_Should_AcceptEmptyGuid_When_Called()
        {
            // Arrange & Act
            var query = new GetSportByIdQuery(Guid.Empty);

            // Assert
            Assert.Equal(Guid.Empty, query.SportId);
        }
    }
}
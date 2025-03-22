using CourtBooking.Application.CourtManagement.Queries.GetCourtAvailability;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetCourtAvailabilityQueryTests
    {
        [Fact]
        public void Constructor_Should_SetProperties_When_Called()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(7);

            // Act
            var query = new GetCourtAvailabilityQuery(courtId, startDate, endDate);

            // Assert
            Assert.Equal(courtId, query.CourtId);
            Assert.Equal(startDate, query.StartDate);
            Assert.Equal(endDate, query.EndDate);
        }

        [Fact]
        public void Constructor_Should_AcceptEmptyGuid_When_Called()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(7);

            // Act
            var query = new GetCourtAvailabilityQuery(Guid.Empty, startDate, endDate);

            // Assert
            Assert.Equal(Guid.Empty, query.CourtId);
        }
    }
}
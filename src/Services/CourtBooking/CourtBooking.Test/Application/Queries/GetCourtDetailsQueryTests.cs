using CourtBooking.Application.CourtManagement.Queries.GetCourtDetails;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetCourtDetailsQueryTests
    {
        [Fact]
        public void Constructor_Should_SetCourtId_When_Called()
        {
            // Arrange
            var courtId = Guid.NewGuid();

            // Act
            var query = new GetCourtDetailsQuery(courtId);

            // Assert
            Assert.Equal(courtId, query.CourtId);
        }

        [Fact]
        public void Constructor_Should_AcceptEmptyGuid()
        {
            // Since the record doesn't validate inputs, we need to adjust our test expectation
            // Arrange & Act
            var query = new GetCourtDetailsQuery(Guid.Empty);

            // Assert
            Assert.Equal(Guid.Empty, query.CourtId);
            // The actual validation should happen in the handler, not in the query constructor
        }
    }
}
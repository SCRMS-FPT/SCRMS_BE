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
        public void Constructor_Should_ThrowArgumentException_When_CourtIdIsEmpty()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => new GetCourtDetailsQuery(Guid.Empty)
            );

            Assert.Contains("rỗng", exception.Message);
        }
    }
}
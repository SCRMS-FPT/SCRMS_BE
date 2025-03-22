using CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetCourtSchedulesByCourtIdQueryTests
    {
        [Fact]
        public void Constructor_Should_SetCourtId_When_Called()
        {
            // Arrange
            var courtId = Guid.NewGuid();

            // Act
            var query = new GetCourtSchedulesByCourtIdQuery(courtId);

            // Assert
            Assert.Equal(courtId, query.CourtId);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentException_When_CourtIdIsEmpty()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => new GetCourtSchedulesByCourtIdQuery(Guid.Empty)
            );

            Assert.Contains("rỗng", exception.Message);
        }
    }
}
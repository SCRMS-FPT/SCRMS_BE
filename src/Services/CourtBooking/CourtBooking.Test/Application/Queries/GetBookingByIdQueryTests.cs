using CourtBooking.Application.BookingManagement.Queries.GetBookingById;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetBookingByIdQueryTests
    {
        [Fact]
        public void Constructor_Should_Create_When_Called()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userRole = "Client";

            // Act
            var query = new GetBookingByIdQuery(bookingId, userId, userRole);

            // Assert
            Assert.NotNull(query);
            Assert.Equal(bookingId, query.BookingId);
            Assert.Equal(userId, query.UserId);
            Assert.Equal(userRole, query.Role);
        }

        [Fact]
        public void Properties_Should_ReturnCorrectValues_When_Accessed()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userRole = "Client";

            // Act
            var query = new GetBookingByIdQuery(bookingId, userId, userRole);

            // Assert
            Assert.Equal(bookingId, query.BookingId);
            Assert.Equal(userId, query.UserId);
            Assert.Equal(userRole, query.Role);
        }
    }
}
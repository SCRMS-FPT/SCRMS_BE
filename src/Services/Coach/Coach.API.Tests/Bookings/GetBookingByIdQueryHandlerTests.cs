using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Bookings.GetBookingById;
using Coach.API.Features.Bookings.UpdateBookingStatus;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Coach.API.Tests.Bookings
{
    public class GetBookingByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingBooking_ReturnsBookingDetail()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = new CoachBooking
            {
                Id = bookingId,
                UserId = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                SportId = Guid.NewGuid(),
                BookingDate = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)),
                Status = "pending",
                TotalPrice = 50,
                PackageId = null
            };

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var handler = new GetBookingByIdQueryHandler(mockBookingRepo.Object);

            // Act
            var result = await handler.Handle(new GetBookingByIdQuery(bookingId), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookingId, result.Id);
            Assert.Equal(booking.UserId, result.UserId);
            Assert.Equal(booking.CoachId, result.CoachId);
            Assert.Equal(booking.SportId, result.SportId);
            Assert.Equal(booking.BookingDate, result.BookingDate);
            Assert.Equal(booking.StartTime, result.StartTime);
            Assert.Equal(booking.EndTime, result.EndTime);
            Assert.Equal(booking.Status, result.Status);
            Assert.Equal(booking.TotalPrice, result.TotalPrice);
            Assert.Equal(booking.PackageId, result.PackageId);
        }

        [Fact]
        public async Task Handle_NonExistingBooking_ThrowsNotFoundException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CoachBooking)null);

            var handler = new GetBookingByIdQueryHandler(mockBookingRepo.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new GetBookingByIdQuery(bookingId), CancellationToken.None));

            Assert.Equal("Booking not found", exception.Message);
        }
    }
}
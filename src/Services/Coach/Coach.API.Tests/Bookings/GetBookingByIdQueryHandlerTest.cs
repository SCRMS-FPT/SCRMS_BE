using Coach.API.Data;
using Coach.API.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildingBlocks.Exceptions;
using System.Threading.Tasks;
using Coach.API.Features.Bookings.GetBookingById;
using Coach.API.Data.Models;
using Coach.API.Features.Bookings.UpdateBookingStatus;

namespace Coach.API.Tests.Bookings
{
    public class GetBookingByIdQueryHandlerTest
    {
        [Fact]
        public async Task Handle_ExistingBooking_ReturnsBookingDetail()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = new CoachBooking { Id = bookingId, UserId = Guid.NewGuid(), CoachId = Guid.NewGuid(), SportId = Guid.NewGuid(), BookingDate = DateOnly.FromDateTime(DateTime.Today), StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)), EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), Status = "pending", TotalPrice = 50, PackageId = null };
            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
            var handler = new GetBookingByIdQueryHandler(mockBookingRepo.Object);

            // Act
            var result = await handler.Handle(new GetBookingByIdQuery(bookingId), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookingId, result.Id);
            Assert.Equal(booking.Status, result.Status);
        }

        [Fact]
        public async Task Handle_NonExistingBooking_ThrowsNotFoundException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync((CoachBooking)null);
            var handler = new GetBookingByIdQueryHandler(mockBookingRepo.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetBookingByIdQuery(bookingId), CancellationToken.None));
            Assert.Equal("Booking not found", exception.Message);
        }
        [Fact]
        public async Task Handle_ValidUpdate_UpdatesStatus()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var booking = new CoachBooking { Id = bookingId, CoachId = coachId, Status = "pending" };
            var command = new UpdateBookingStatusCommand(bookingId, "completed", coachId);
            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
            var mockContext = new Mock<CoachDbContext>();
            var handler = new UpdateBookingStatusCommandHandler(mockBookingRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockBookingRepo.Verify(repo => repo.UpdateCoachBookingAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(result.IsUpdated);
            Assert.Equal("completed", booking.Status);
        }
    }
}
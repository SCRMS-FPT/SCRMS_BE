using Coach.API.Features.Bookings.UpdateBookingStatus;
using Coach.API.Data.Repositories;
using Coach.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using FluentValidation;
using Coach.API.Data.Models;
using Moq;
using Xunit;

namespace Coach.API.Tests.Bookings
{
    public class UpdateBookingStatusCommandHandlerTest
    {
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

        [Fact]
        public async Task Handle_NonExistingBooking_ThrowsNotFoundException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var command = new UpdateBookingStatusCommand(bookingId, "completed", Guid.NewGuid());
            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync((CoachBooking)null);
            var handler = new UpdateBookingStatusCommandHandler(mockBookingRepo.Object, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Booking not found", exception.Message);
        }

        [Fact]
        public async Task Handle_WrongCoach_ThrowsBadRequestException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var booking = new CoachBooking { Id = bookingId, CoachId = Guid.NewGuid() }; // Different CoachId
            var command = new UpdateBookingStatusCommand(bookingId, "completed", coachId);
            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
            var handler = new UpdateBookingStatusCommandHandler(mockBookingRepo.Object, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Booking coach is not you", exception.Message);
        }

        [Fact]
        public void Validate_InvalidStatus_ValidationFails()
        {
            // Arrange
            var command = new UpdateBookingStatusCommand(Guid.NewGuid(), "invalid", Guid.NewGuid());
            var validator = new UpdateBookingStatusCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Status must be either 'completed' or 'cancelled'.");
        }
    }
}
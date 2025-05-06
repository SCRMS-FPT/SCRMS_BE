using System;
using System.Threading;
using System.Threading.Tasks;
using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Bookings.UpdateBookingStatus;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentValidation;

namespace Coach.API.Tests.Bookings
{
    public class UpdateBookingStatusCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidUpdate_UpdatesStatus()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var booking = new CoachBooking
            {
                Id = bookingId,
                CoachId = coachId,
                Status = "pending"
            };

            var command = new UpdateBookingStatusCommand(bookingId, "completed", coachId);

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            mockBookingRepo.Setup(repo => repo.UpdateCoachBookingAsync(It.IsAny<CoachBooking>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new UpdateBookingStatusCommandHandler(mockBookingRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockBookingRepo.Verify(repo => repo.UpdateCoachBookingAsync(
                It.Is<CoachBooking>(b => b.Id == bookingId && b.Status == "completed"),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.True(result.IsUpdated);
            Assert.Equal("completed", booking.Status);
        }

        [Fact]
        public async Task Handle_BookingNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var command = new UpdateBookingStatusCommand(bookingId, "completed", coachId);

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CoachBooking)null);

            var mockContext = new Mock<CoachDbContext>();

            var handler = new UpdateBookingStatusCommandHandler(mockBookingRepo.Object, mockContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            mockBookingRepo.Verify(repo => repo.UpdateCoachBookingAsync(
                It.IsAny<CoachBooking>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_UnauthorizedCoach_ThrowsBadRequestException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var actualCoachId = Guid.NewGuid();
            var requestingCoachId = Guid.NewGuid();  // Different coach ID

            var booking = new CoachBooking
            {
                Id = bookingId,
                CoachId = actualCoachId,  // This is the actual coach for the booking
                Status = "pending"
            };

            var command = new UpdateBookingStatusCommand(
                bookingId,
                "completed",
                requestingCoachId  // Different coach trying to update
            );

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var mockContext = new Mock<CoachDbContext>();

            var handler = new UpdateBookingStatusCommandHandler(mockBookingRepo.Object, mockContext.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Booking coach is not you", exception.Message);

            mockBookingRepo.Verify(repo => repo.UpdateCoachBookingAsync(
                It.IsAny<CoachBooking>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData("pending", "completed")]
        [InlineData("pending", "cancelled")]
        public async Task Handle_ValidStatusTransition_UpdatesStatus(string initialStatus, string newStatus)
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var booking = new CoachBooking
            {
                Id = bookingId,
                CoachId = coachId,
                Status = initialStatus
            };

            var command = new UpdateBookingStatusCommand(bookingId, newStatus, coachId);

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
            mockBookingRepo.Setup(repo => repo.UpdateCoachBookingAsync(It.IsAny<CoachBooking>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new UpdateBookingStatusCommandHandler(mockBookingRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsUpdated);
            Assert.Equal(newStatus, booking.Status);
        }

        [Theory]
        [InlineData("cancelled", "completed")]  // Can't complete a cancelled booking
        [InlineData("completed", "pending")]    // Can't revert a completed booking
        [InlineData("completed", "cancelled")]  // Can't cancel a completed booking
        public async Task Handle_InvalidStatusTransition_ThrowsBadRequestException(string initialStatus, string newStatus)
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var booking = new CoachBooking
            {
                Id = bookingId,
                CoachId = coachId,
                Status = initialStatus
            };

            var command = new UpdateBookingStatusCommand(bookingId, newStatus, coachId);

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.GetCoachBookingByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var mockContext = new Mock<CoachDbContext>();

            var handler = new UpdateBookingStatusCommandHandler(mockBookingRepo.Object, mockContext.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Invalid booking status", exception.Message);
        }
    }
}
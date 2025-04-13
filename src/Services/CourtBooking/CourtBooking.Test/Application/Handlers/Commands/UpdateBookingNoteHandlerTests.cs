using CourtBooking.Application.BookingManagement.Command.UpdateBookingNote;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Commands
{
    public class UpdateBookingNoteHandlerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly UpdateBookingNoteHandler _handler;

        public UpdateBookingNoteHandlerTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _handler = new UpdateBookingNoteHandler(_mockBookingRepository.Object);
        }

        [Fact]
        public async Task Handle_Should_UpdateBookingNote_When_UserIsAuthorized()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var newNote = "Updated note for booking";

            var command = new UpdateBookingNoteCommand(
                bookingId,
                userId,
                newNote
            );

            // Setup booking
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(userId),
                DateTime.Now.AddDays(1),
                "Original note"
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newNote, booking.Note);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_BookingNotFound()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var newNote = "Updated note for booking";

            var command = new UpdateBookingNoteCommand(
                bookingId,
                userId,
                newNote
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Booking not found", result.ErrorMessage);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorized()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var newNote = "Updated note for booking";

            var command = new UpdateBookingNoteCommand(
                bookingId,
                userId,
                newNote
            );

            // Setup booking with different owner
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(differentUserId), // Different user
                DateTime.Now.AddDays(1),
                "Original note"
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("You are not authorized to update this booking note", result.ErrorMessage);
            Assert.Equal("Original note", booking.Note); // Note should not change
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_UpdateBookingNote_When_NoteIsEmpty()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var newNote = string.Empty; // Empty note

            var command = new UpdateBookingNoteCommand(
                bookingId,
                userId,
                newNote
            );

            // Setup booking
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(userId),
                DateTime.Now.AddDays(1),
                "Original note"
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(string.Empty, booking.Note);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 
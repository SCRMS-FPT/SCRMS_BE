using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Outbox;
using CourtBooking.Application.BookingManagement.Command.CancelBooking;
using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using DomainEvent = CourtBooking.Domain.Abstractions.IDomainEvent;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Commands
{
    public class CancelBookingHandlerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly Mock<ISportCenterRepository> _mockSportCenterRepository;
        private readonly Mock<IOutboxService> _mockOutboxService;
        private readonly Mock<IApplicationDbContext> _mockDbContext;
        private readonly Mock<IDbContextTransaction> _mockTransaction;
        private readonly CancelBookingCommandHandler _handler;

        public CancelBookingHandlerTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockCourtRepository = new Mock<ICourtRepository>();
            _mockSportCenterRepository = new Mock<ISportCenterRepository>();
            _mockOutboxService = new Mock<IOutboxService>();
            _mockDbContext = new Mock<IApplicationDbContext>();
            _mockTransaction = new Mock<IDbContextTransaction>();

            _mockDbContext.Setup(db => db.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockTransaction.Object);

            _handler = new CancelBookingCommandHandler(
                _mockBookingRepository.Object,
                _mockCourtRepository.Object,
                _mockSportCenterRepository.Object,
                _mockOutboxService.Object,
                _mockDbContext.Object
            );
        }

        [Fact]
        public async Task Handle_Should_CancelBooking_When_UserIsBookingOwner()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var requestedAt = DateTime.Now;
            var command = new CancelBookingCommand(
                bookingId,
                "Test cancellation reason",
                requestedAt,
                userId,
                "User"
            );

            // Setup booking
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(userId),
                DateTime.Now.AddDays(1),
                "Test booking"
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockOutboxService.Verify(o => o.PublishAsync<DomainEvent>(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_CancelBooking_When_UserIsSportCenterOwner()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var requestedAt = DateTime.Now;
            var command = new CancelBookingCommand(
                bookingId,
                "Test cancellation reason",
                requestedAt,
                userId,
                "SportCenterOwner"
            );

            // Setup booking owned by another user
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(anotherUserId),
                DateTime.Now.AddDays(1),
                "Test booking"
            );

            // Add booking detail with court
            booking.AddBookingDetail(
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>(),
                0
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Setup court is owned by sport center
            _mockCourtRepository.Setup(r => r.GetSportCenterIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenterId);

            // Setup sport center is owned by userId
            _mockSportCenterRepository.Setup(r => r.IsOwnedByUserAsync(sportCenterId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockOutboxService.Verify(o => o.PublishAsync<DomainEvent>(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_When_BookingDoesNotExist()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var requestedAt = DateTime.Now;
            var command = new CancelBookingCommand(
                bookingId,
                "Test cancellation reason",
                requestedAt,
                userId,
                "User"
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            _mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_When_BookingAlreadyCancelled()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var requestedAt = DateTime.Now;
            var command = new CancelBookingCommand(
                bookingId,
                "Test cancellation reason",
                requestedAt,
                userId,
                "User"
            );

            // Setup booking already cancelled
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(userId),
                DateTime.Now.AddDays(1),
                "Test booking"
            );
            booking.UpdateStatus(BookingStatus.Cancelled);

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            Assert.Contains("đã bị hủy", exception.Message);
            _mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_When_UserUnauthorized()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var requestedAt = DateTime.Now;
            var command = new CancelBookingCommand(
                bookingId,
                "Test cancellation reason",
                requestedAt,
                userId,
                "User"
            );

            // Setup booking owned by another user
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(anotherUserId),
                DateTime.Now.AddDays(1),
                "Test booking"
            );

            // Add booking detail with court
            booking.AddBookingDetail(
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>(),
                0
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Setup court is owned by sport center
            _mockCourtRepository.Setup(r => r.GetSportCenterIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenterId);

            // Setup sport center is NOT owned by userId
            _mockSportCenterRepository.Setup(r => r.IsOwnedByUserAsync(sportCenterId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            _mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
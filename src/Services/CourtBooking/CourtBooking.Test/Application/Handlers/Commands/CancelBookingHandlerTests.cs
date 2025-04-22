using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Outbox;
using CourtBooking.Application.BookingManagement.Command.CancelBooking;
using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
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

            _mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

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
            var courtId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
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

            // Add booking detail with court
            var detail = BookingDetail.Create(
                booking.Id,
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );

            var bookingDetails = new List<BookingDetail> { detail };

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Setup booking details
            _mockBookingRepository.Setup(r => r.GetBookingDetailsAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookingDetails);

            // Setup court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test court",
                "[]",
                CourtType.Indoor,
                50, 24, 100
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Setup sport center
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(Guid.NewGuid()),
                "Test Sport Center",
                "0123456789",
                new Location("123 Street", "City", "Country", "12345"),
                new GeoLocation(0, 0),
                new SportCenterImages("test.jpg", new List<string>()),
                "Description"
            );

            _mockSportCenterRepository.Setup(r => r.GetSportCenterByIdAsync(It.Is<SportCenterId>(s => s.Value == sportCenterId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenter);

            // Setup save changes
            _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.Equal(bookingId, result.BookingId);
            Assert.Equal("Cancelled", result.Status);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockOutboxService.Verify(o => o.SaveMessageAsync(It.IsAny<object>()), Times.AtLeastOnce);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_CancelBooking_When_UserIsSportCenterOwner()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var requestedAt = DateTime.Now;
            var command = new CancelBookingCommand(
                bookingId,
                "Test cancellation reason",
                requestedAt,
                ownerId,
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
            var detail = BookingDetail.Create(
                booking.Id,
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );

            var bookingDetails = new List<BookingDetail> { detail };

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Setup booking details
            _mockBookingRepository.Setup(r => r.GetBookingDetailsAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookingDetails);

            // Setup court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test court",
                "[]",
                CourtType.Indoor,
                50, 24, 100
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Setup sport center owned by ownerId
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(ownerId),
                "Test Sport Center",
                "0123456789",
                new Location("123 Street", "City", "Country", "12345"),
                new GeoLocation(0, 0),
                new SportCenterImages("test.jpg", new List<string>()),
                "Description"
            );

            _mockSportCenterRepository.Setup(r => r.GetSportCenterByIdAsync(It.Is<SportCenterId>(s => s.Value == sportCenterId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenter);

            // Setup is owned by user check
            _mockSportCenterRepository.Setup(r => r.IsOwnedByUserAsync(sportCenterId, ownerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Setup save changes
            _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.Equal(bookingId, result.BookingId);
            Assert.Equal("Cancelled", result.Status);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
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
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            _mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_When_BookingAlreadyCancelled()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
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

            // Add booking detail with court
            var detail = BookingDetail.Create(
                booking.Id,
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );

            var bookingDetails = new List<BookingDetail> { detail };

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Setup booking details
            _mockBookingRepository.Setup(r => r.GetBookingDetailsAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookingDetails);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            Assert.Contains("already cancelled", exception.Message);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_When_UserUnauthorized()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var sportCenterOwnerId = Guid.NewGuid(); // Different from userId
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
            var detail = BookingDetail.Create(
                booking.Id,
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );

            var bookingDetails = new List<BookingDetail> { detail };

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Setup booking details
            _mockBookingRepository.Setup(r => r.GetBookingDetailsAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookingDetails);

            // Setup court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test court",
                "[]",
                CourtType.Indoor,
                50, 24, 100
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Set up the sport center owned by someone else
            _mockSportCenterRepository.Setup(r => r.IsOwnedByUserAsync(sportCenterId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );
        }
    }
}
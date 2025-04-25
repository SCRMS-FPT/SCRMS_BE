using CourtBooking.Application.BookingManagement.Command.UpdateBookingStatus;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Commands
{
    public class UpdateBookingStatusHandlerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly Mock<ISportCenterRepository> _mockSportCenterRepository;
        private readonly UpdateBookingStatusHandler _handler;

        public UpdateBookingStatusHandlerTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockCourtRepository = new Mock<ICourtRepository>();
            _mockSportCenterRepository = new Mock<ISportCenterRepository>();

            _handler = new UpdateBookingStatusHandler(
                _mockBookingRepository.Object,
                _mockCourtRepository.Object,
                _mockSportCenterRepository.Object
            );
        }

        [Fact]
        public async Task Handle_Should_UpdateBookingStatus_When_OwnerIsAuthorized()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var courtId = Guid.NewGuid();

            var command = new UpdateBookingStatusCommand(
                BookingId: bookingId,
                OwnerId: ownerId,
                Status: BookingStatus.Deposited // Changed from Confirmed to Deposited
            );

            // Setup booking
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(Guid.NewGuid()),
                DateTime.Now.AddDays(1),
                "Test booking"
            );

            // Add booking detail
            var bookingDetail = BookingDetail.Create(
                BookingId.Of(bookingId),
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );

            // Add booking detail to booking
            var bookingDetailsField = typeof(Booking).GetField("_bookingDetails",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bookingDetailsField?.SetValue(booking, new List<BookingDetail> { bookingDetail });

            // Setup court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1),
                "Main court",
                "Indoor",
                CourtType.Indoor,
                30
            );

            // Setup sport center
            var location = new Location("123 Main St", "District", "City", "Country");
            var geoLocation = new GeoLocation(10.0, 20.0);
            var images = new SportCenterImages("avatar.jpg", new List<string>());
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(ownerId),
                "Sport Center 1",
                "123456789",
                location,
                geoLocation,
                images,
                "Description"
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockSportCenterRepository.Setup(r => r.GetSportCenterByIdAsync(It.IsAny<SportCenterId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenter);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(BookingStatus.Deposited, booking.Status); // Changed from Confirmed to Deposited
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_BookingNotFound()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var command = new UpdateBookingStatusCommand(
                BookingId: bookingId,
                OwnerId: ownerId,
                Status: BookingStatus.Completed
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
        public async Task Handle_Should_ReturnFailure_When_OwnerIsNotAuthorized()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var differentOwnerId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var courtId = Guid.NewGuid();

            var command = new UpdateBookingStatusCommand(
                BookingId: bookingId,
                OwnerId: ownerId,
                Status: BookingStatus.Completed
            );

            // Setup booking
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(Guid.NewGuid()),
                DateTime.Now.AddDays(1),
                "Test booking"
            );

            // Add booking detail
            var bookingDetail = BookingDetail.Create(
                BookingId.Of(bookingId),
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );

            // Add booking detail to booking
            var bookingDetailsField = typeof(Booking).GetField("_bookingDetails",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bookingDetailsField?.SetValue(booking, new List<BookingDetail> { bookingDetail });

            // Setup court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1),
                "Main court",
                "Indoor",
                CourtType.Indoor,
                30
            );

            // Setup sport center with different owner
            var location = new Location("123 Main St", "District", "City", "Country");
            var geoLocation = new GeoLocation(10.0, 20.0);
            var images = new SportCenterImages("avatar.jpg", new List<string>());
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(differentOwnerId),
                "Sport Center 1",
                "123456789",
                location,
                geoLocation,
                images,
                "Description"
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockSportCenterRepository.Setup(r => r.GetSportCenterByIdAsync(It.IsAny<SportCenterId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenter);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("You are not authorized to update this booking", result.ErrorMessage);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_StatusTransitionIsInvalid()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var courtId = Guid.NewGuid();

            var command = new UpdateBookingStatusCommand(
                BookingId: bookingId,
                OwnerId: ownerId,
                Status: BookingStatus.PaymentFail // Invalid transition from Completed
            );

            // Setup booking with Completed status
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(Guid.NewGuid()),
                DateTime.Now.AddDays(1),
                "Test booking"
            );
            booking.UpdateStatus(BookingStatus.Completed); // Set to completed

            // Add booking detail
            var bookingDetail = BookingDetail.Create(
                BookingId.Of(bookingId),
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );

            // Add booking detail to booking
            var bookingDetailsField = typeof(Booking).GetField("_bookingDetails",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bookingDetailsField?.SetValue(booking, new List<BookingDetail> { bookingDetail });

            // Setup court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1),
                "Main court",
                "Indoor",
                CourtType.Indoor,
                30
            );

            // Setup sport center
            var location = new Location("123 Main St", "District", "City", "Country");
            var geoLocation = new GeoLocation(10.0, 20.0);
            var images = new SportCenterImages("avatar.jpg", new List<string>());
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(ownerId),
                "Sport Center 1",
                "123456789",
                location,
                geoLocation,
                images,
                "Description"
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockSportCenterRepository.Setup(r => r.GetSportCenterByIdAsync(It.IsAny<SportCenterId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenter);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Invalid status transition", result.ErrorMessage);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_UpdateBookingStatus_When_ValidStatusTransition()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var courtId = Guid.NewGuid();

            var command = new UpdateBookingStatusCommand(
                BookingId: bookingId,
                OwnerId: ownerId,
                Status: BookingStatus.Cancelled
            );

            // Setup booking with PendingPayment status
            var booking = Booking.Create(
                BookingId.Of(bookingId),
                UserId.Of(Guid.NewGuid()),
                DateTime.Now.AddDays(1),
                "Test booking"
            );
            booking.UpdateStatus(BookingStatus.PendingPayment); // Use PendingPayment instead of Pending

            // Add booking detail
            var bookingDetail = BookingDetail.Create(
                BookingId.Of(bookingId),
                CourtId.Of(courtId),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );

            // Add booking detail to booking
            var bookingDetailsField = typeof(Booking).GetField("_bookingDetails",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bookingDetailsField?.SetValue(booking, new List<BookingDetail> { bookingDetail });

            // Setup court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1),
                "Main court",
                "Indoor",
                CourtType.Indoor,
                30
            );

            // Setup sport center
            var location = new Location("123 Main St", "District", "City", "Country");
            var geoLocation = new GeoLocation(10.0, 20.0);
            var images = new SportCenterImages("avatar.jpg", new List<string>());
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(ownerId),
                "Sport Center 1",
                "123456789",
                location,
                geoLocation,
                images,
                "Description"
            );

            _mockBookingRepository.Setup(r => r.GetBookingByIdAsync(It.IsAny<BookingId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockSportCenterRepository.Setup(r => r.GetSportCenterByIdAsync(It.IsAny<SportCenterId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenter);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            _mockBookingRepository.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
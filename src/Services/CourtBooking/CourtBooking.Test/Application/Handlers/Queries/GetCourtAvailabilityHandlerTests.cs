using CourtBooking.Application.CourtManagement.Queries.GetCourtAvailability;
using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Exceptions;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetCourtAvailabilityHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly Mock<ICourtScheduleRepository> _mockCourtScheduleRepository;
        private readonly Mock<ICourtPromotionRepository> _mockCourtPromotionRepository;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly GetCourtAvailabilityHandler _handler;

        public GetCourtAvailabilityHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockCourtRepository = new Mock<ICourtRepository>();
            _mockCourtScheduleRepository = new Mock<ICourtScheduleRepository>();
            _mockCourtPromotionRepository = new Mock<ICourtPromotionRepository>();
            _mockBookingRepository = new Mock<IBookingRepository>();

            _handler = new GetCourtAvailabilityHandler(
                _mockContext.Object,
                _mockCourtRepository.Object,
                _mockCourtScheduleRepository.Object,
                _mockCourtPromotionRepository.Object,
                _mockBookingRepository.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ThrowDomainException_When_CourtNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(1);

            var query = new GetCourtAvailabilityQuery(courtId, startDate, endDate);

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Court)null);

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(
                () => _handler.Handle(query, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_Should_ThrowDomainException_When_EndDateBeforeStartDate()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today.AddDays(1);
            var endDate = DateTime.Today; // Before start date

            var query = new GetCourtAvailabilityQuery(courtId, startDate, endDate);

            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1),
                "Main court",
                "Indoor",
                CourtType.Indoor, // Use enum value for clarity
                30
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(
                () => _handler.Handle(query, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_Should_ThrowDomainException_When_DateRangeExceeds31Days()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(32); // More than 31 days

            var query = new GetCourtAvailabilityQuery(courtId, startDate, endDate);

            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1.5),
                "Main court",
                "Indoor",
                CourtType.Indoor, // Use enum value instead of 1
                30
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(
                () => _handler.Handle(query, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptySchedule_When_NoSchedulesFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today;

            var query = new GetCourtAvailabilityQuery(courtId, startDate, endDate);

            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1), // Added slotDuration to match signature
                "Main court",
                "Indoor",
                CourtType.Indoor, // Use enum value instead of 1
                30
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockCourtScheduleRepository.Setup(r => r.GetSchedulesByCourt(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule>());

            _mockCourtPromotionRepository.Setup(r => r.GetValidPromotionsForCourtAsync(
                    It.IsAny<CourtId>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            _mockBookingRepository.Setup(r => r.GetBookingsInDateRangeForCourtAsync(
                    It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(courtId, result.CourtId);
            Assert.Single(result.Schedule);
            Assert.Equal(startDate.Date, result.Schedule[0].Date.Date);
            Assert.Empty(result.Schedule[0].TimeSlots);
        }

        [Fact]
        public async Task Handle_Should_ReturnAvailableTimeSlots_When_ScheduleExists()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today; // e.g., Monday
            var endDate = DateTime.Today;
            var dayOfWeek = ((int)startDate.DayOfWeek + 6) % 7 + 1; // Convert to 1-7 format

            var query = new GetCourtAvailabilityQuery(courtId, startDate, endDate);

            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1), // 1-hour time slots
                "Main court",
                "Indoor",
                CourtType.Indoor,
                30
            );

            var schedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                CourtId.Of(courtId),
                DayOfWeekValue.Of(new List<int> { dayOfWeek }), // For the test day
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(10),
                150.0m
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockCourtScheduleRepository.Setup(r => r.GetSchedulesByCourt(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule> { schedule });

            _mockCourtPromotionRepository.Setup(r => r.GetValidPromotionsForCourtAsync(
                    It.IsAny<CourtId>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            // Tạo danh sách bookings trống cho test case này
            var bookings = new List<Booking>();
            _mockBookingRepository.Setup(r => r.GetBookingsInDateRangeForCourtAsync(
                    courtId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(courtId, result.CourtId);
            Assert.Single(result.Schedule);
            Assert.Equal(startDate.Date, result.Schedule[0].Date.Date);
            Assert.Equal(dayOfWeek, result.Schedule[0].DayOfWeek);

            // Với lịch trình từ 8:00-10:00 và thời lượng sân 1 giờ, chúng ta kỳ vọng 2 time slots
            Assert.Equal(2, result.Schedule[0].TimeSlots.Count);

            // Kiểm tra time slot đầu tiên: 8:00-9:00
            Assert.Equal("AVAILABLE", result.Schedule[0].TimeSlots[0].Status);
            Assert.Equal("08:00", result.Schedule[0].TimeSlots[0].StartTime);
            Assert.Equal("09:00", result.Schedule[0].TimeSlots[0].EndTime);
            Assert.Equal(150.0m, result.Schedule[0].TimeSlots[0].Price);

            // Kiểm tra time slot thứ hai: 9:00-10:00
            Assert.Equal("AVAILABLE", result.Schedule[0].TimeSlots[1].Status);
            Assert.Equal("09:00", result.Schedule[0].TimeSlots[1].StartTime);
            Assert.Equal("10:00", result.Schedule[0].TimeSlots[1].EndTime);
            Assert.Equal(150.0m, result.Schedule[0].TimeSlots[1].Price);
        }

        [Fact]
        public async Task Handle_Should_MarkSlotsAsBooked_When_BookingsExist()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today;
            var dayOfWeek = ((int)startDate.DayOfWeek + 6) % 7 + 1;

            var query = new GetCourtAvailabilityQuery(courtId, startDate, endDate);

            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1), // 1-hour time slots
                "Main court",
                "Indoor",
                CourtType.Indoor,
                30
            );

            var schedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                CourtId.Of(courtId),
                DayOfWeekValue.Of(new List<int> { dayOfWeek }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(10),
                150.0m
            );

            // Create a booking for the first time slot on the test day
            var bookingId = BookingId.Of(Guid.NewGuid());
            var booking = Booking.Create(
                bookingId,
                UserId.Of(userId),
                startDate,
                "Test booking"
            );

            // Thiết lập status là Deposited
            booking.UpdateStatus(BookingStatus.Deposited); // Changed from Confirmed to Deposited

            var bookingDetail = BookingDetail.Create(
                bookingId,
                CourtId.Of(courtId),
                TimeSpan.FromHours(8),  // First time slot: 8:00 - 9:00
                TimeSpan.FromHours(9),
                new List<CourtSchedule> { schedule }
            );

            // Add booking detail to booking
            var bookingDetailsList = new List<BookingDetail> { bookingDetail };
            typeof(Booking).GetField("_bookingDetails", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(booking, bookingDetailsList);

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockCourtScheduleRepository.Setup(r => r.GetSchedulesByCourt(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule> { schedule });

            _mockCourtPromotionRepository.Setup(r => r.GetValidPromotionsForCourtAsync(
                    It.IsAny<CourtId>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            _mockBookingRepository.Setup(r => r.GetBookingsInDateRangeForCourtAsync(
                    courtId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking> { booking });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(courtId, result.CourtId);
            Assert.Single(result.Schedule);
            Assert.Equal(2, result.Schedule[0].TimeSlots.Count);

            // First slot should be booked
            Assert.Equal("BOOKED", result.Schedule[0].TimeSlots[0].Status);
            Assert.Equal(userId.ToString(), result.Schedule[0].TimeSlots[0].BookedBy);
            Assert.Equal("08:00", result.Schedule[0].TimeSlots[0].StartTime);
            Assert.Equal("09:00", result.Schedule[0].TimeSlots[0].EndTime);

            // Second slot should be available
            Assert.Equal("AVAILABLE", result.Schedule[0].TimeSlots[1].Status);
            Assert.Null(result.Schedule[0].TimeSlots[1].BookedBy);
            Assert.Equal("09:00", result.Schedule[0].TimeSlots[1].StartTime);
            Assert.Equal("10:00", result.Schedule[0].TimeSlots[1].EndTime);
        }

        [Fact]
        public async Task Handle_Should_ApplyPromotion_When_PromotionExists()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today;
            var dayOfWeek = ((int)startDate.DayOfWeek + 6) % 7 + 1;

            var query = new GetCourtAvailabilityQuery(courtId, startDate, endDate);

            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1), // 1-hour time slots
                "Main court",
                "Indoor",
                CourtType.Indoor,
                30
            );

            var schedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                CourtId.Of(courtId),
                DayOfWeekValue.Of(new List<int> { dayOfWeek }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(10),
                150.0m
            );

            var promotion = CourtPromotion.Create(
                CourtId.Of(courtId),
                "Discount for summer season",
                "Percentage",
                20.0m,
                startDate.AddDays(-1),
                endDate.AddDays(1)
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockCourtScheduleRepository.Setup(r => r.GetSchedulesByCourt(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule> { schedule });

            _mockCourtPromotionRepository.Setup(r => r.GetValidPromotionsForCourtAsync(
                    It.IsAny<CourtId>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion> { promotion });

            // Tạo danh sách bookings trống
            var bookings = new List<Booking>();
            _mockBookingRepository.Setup(r => r.GetBookingsInDateRangeForCourtAsync(
                    courtId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(courtId, result.CourtId);
            Assert.Single(result.Schedule);
            Assert.Equal(2, result.Schedule[0].TimeSlots.Count);

            // Kiểm tra slot đầu tiên (8:00-9:00)
            Assert.NotNull(result.Schedule[0].TimeSlots[0].Promotion);
            Assert.Equal("Percentage", result.Schedule[0].TimeSlots[0].Promotion.DiscountType);
            Assert.Equal(20.0m, result.Schedule[0].TimeSlots[0].Promotion.DiscountValue);
            Assert.Equal("08:00", result.Schedule[0].TimeSlots[0].StartTime);
            Assert.Equal("09:00", result.Schedule[0].TimeSlots[0].EndTime);

            // Kiểm tra slot thứ hai (9:00-10:00)
            Assert.NotNull(result.Schedule[0].TimeSlots[1].Promotion);
            Assert.Equal("Percentage", result.Schedule[0].TimeSlots[1].Promotion.DiscountType);
            Assert.Equal(20.0m, result.Schedule[0].TimeSlots[1].Promotion.DiscountValue);
            Assert.Equal("09:00", result.Schedule[0].TimeSlots[1].StartTime);
            Assert.Equal("10:00", result.Schedule[0].TimeSlots[1].EndTime);
        }

        [Fact]
        public async Task Handle_Should_MarkSlotsAsMaintenance_When_ScheduleInMaintenance()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today;
            var dayOfWeek = ((int)startDate.DayOfWeek + 6) % 7 + 1;

            var query = new GetCourtAvailabilityQuery(courtId, startDate, endDate);

            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1), // 1-hour time slots
                "Main court",
                "Indoor",
                CourtType.Indoor,
                30
            );

            var schedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                CourtId.Of(courtId),
                DayOfWeekValue.Of(new List<int> { dayOfWeek }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(10),
                150.0m
            );

            // Set schedule to maintenance
            schedule.Update(schedule.DayOfWeek, schedule.StartTime, schedule.EndTime,
                schedule.PriceSlot, CourtScheduleStatus.Maintenance);

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockCourtScheduleRepository.Setup(r => r.GetSchedulesByCourt(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule> { schedule });

            _mockCourtPromotionRepository.Setup(r => r.GetValidPromotionsForCourtAsync(
                    It.IsAny<CourtId>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            // Tạo danh sách bookings trống
            var bookings = new List<Booking>();
            _mockBookingRepository.Setup(r => r.GetBookingsInDateRangeForCourtAsync(
                    courtId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(courtId, result.CourtId);
            Assert.Single(result.Schedule);
            Assert.Equal(2, result.Schedule[0].TimeSlots.Count);

            // All slots should be marked as maintenance
            Assert.All(result.Schedule[0].TimeSlots, slot => Assert.Equal("MAINTENANCE", slot.Status));

            // Kiểm tra thời gian của các slot
            Assert.Equal("08:00", result.Schedule[0].TimeSlots[0].StartTime);
            Assert.Equal("09:00", result.Schedule[0].TimeSlots[0].EndTime);
            Assert.Equal("09:00", result.Schedule[0].TimeSlots[1].StartTime);
            Assert.Equal("10:00", result.Schedule[0].TimeSlots[1].EndTime);
        }
    }
}
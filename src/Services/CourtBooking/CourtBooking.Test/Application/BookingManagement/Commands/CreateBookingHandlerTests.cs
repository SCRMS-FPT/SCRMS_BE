using CourtBooking.Application.BookingManagement.Command.CreateBooking;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.BookingManagement.Command.CreateBooking
{
    public class CreateBookingHandlerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<ICourtScheduleRepository> _mockCourtScheduleRepository;
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly CreateBookingHandler _handler;

        public CreateBookingHandlerTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockCourtScheduleRepository = new Mock<ICourtScheduleRepository>();
            _mockCourtRepository = new Mock<ICourtRepository>();

            _handler = new CreateBookingHandler(
                _mockBookingRepository.Object,
                _mockCourtScheduleRepository.Object,
                _mockCourtRepository.Object
            );
        }

        [Fact]
        public async Task Handle_Should_CreateBookingWithCorrectDetails_WhenValid()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(12);

            var command = new CreateBookingCommand(new BookingCreateDTO(
                UserId: userId,
                BookingDate: bookingDate,
                TotalPrice: 200m,
                Note: "Test booking",
                DepositAmount: 50m,
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(courtId, startTime, endTime)
                }
            ));

            SetupMocks(courtId, bookingDate, startTime, endTime);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            _mockBookingRepository.Verify(r => r.AddBookingAsync(
                It.Is<Booking>(b =>
                    b.UserId.Value == userId &&
                    b.BookingDate == bookingDate &&
                    b.BookingDetails.Count == 1 &&
                    b.BookingDetails.First().CourtId.Value == courtId &&
                    b.BookingDetails.First().StartTime == startTime &&
                    b.BookingDetails.First().EndTime == endTime
                ),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_Should_MakeDeposit_WhenDepositAmountProvided()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.Today.AddDays(1);
            var depositAmount = 100m;

            var command = new CreateBookingCommand(new BookingCreateDTO(
                UserId: userId,
                BookingDate: bookingDate,
                TotalPrice: 200m,
                Note: "Test booking",
                DepositAmount: depositAmount,
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(
                        courtId,
                        TimeSpan.FromHours(10),
                        TimeSpan.FromHours(12)
                    )
                }
            ));

            SetupMocks(courtId, bookingDate, TimeSpan.FromHours(10), TimeSpan.FromHours(12));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockBookingRepository.Verify(r => r.AddBookingAsync(
                It.Is<Booking>(b =>
                    b.TotalPaid == depositAmount &&
                    b.Status == BookingStatus.Confirmed
                ),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_WhenCourtNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.Today.AddDays(1);

            var command = new CreateBookingCommand(new BookingCreateDTO(
                UserId: userId,
                BookingDate: bookingDate,
                TotalPrice: 200m,
                Note: "Test booking",
                DepositAmount: 50m,
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(
                        courtId,
                        TimeSpan.FromHours(10),
                        TimeSpan.FromHours(12)
                    )
                }
            ));

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                It.Is<CourtId>(id => id.Value == courtId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Court)null);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Should_ThrowException_WhenNoSchedulesFoundForDay()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.Today.AddDays(1);
            var dayOfWeek = (int)bookingDate.DayOfWeek + 1;

            var command = new CreateBookingCommand(new BookingCreateDTO(
                UserId: userId,
                BookingDate: bookingDate,
                TotalPrice: 200m,
                Note: "Test booking",
                DepositAmount: 50m,
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(
                        courtId,
                        TimeSpan.FromHours(10),
                        TimeSpan.FromHours(12)
                    )
                }
            ));

            // Setup mock court
            var mockCourt = SetupMockCourt(courtId, 50m);

            // Setup mock schedules for a different day
            var differentDayOfWeek = dayOfWeek == 7 ? 1 : dayOfWeek + 1;
            var mockSchedules = new List<CourtSchedule>
            {
                CourtSchedule.Create(
                    CourtScheduleId.Of(Guid.NewGuid()),
                    CourtId.Of(courtId),
                    new DayOfWeekValue(new int[] { differentDayOfWeek }),
                    TimeSpan.FromHours(8),
                    TimeSpan.FromHours(20),
                    100m
                )
            };

            _mockCourtScheduleRepository.Setup(r => r.GetSchedulesByCourtIdAsync(
                It.Is<CourtId>(id => id.Value == courtId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSchedules);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Should_ThrowException_WhenTimeSlotIsAlreadyBooked()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(12);

            var command = new CreateBookingCommand(new BookingCreateDTO(
                UserId: userId,
                BookingDate: bookingDate,
                TotalPrice: 200m,
                Note: "Test booking",
                DepositAmount: 50m,
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(courtId, startTime, endTime)
                }
            ));

            // Setup mock court and schedules
            SetupMocks(courtId, bookingDate, startTime, endTime);

            // Setup existing bookings that conflict with the requested time slot
            var existingBooking = CreateExistingBooking(courtId, bookingDate, startTime, endTime);
            _mockBookingRepository.Setup(r => r.GetBookingsInDateRangeForCourtAsync(
                courtId,
                bookingDate,
                bookingDate,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking> { existingBooking });

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Should_ThrowException_WhenDepositIsTooLow()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(12);
            var minDepositPercentage = 50m; // 50% đặt cọc tối thiểu
            var totalPrice = 200m;
            var tooLowDeposit = (totalPrice * minDepositPercentage / 100) - 1; // Thấp hơn 1 đơn vị so với yêu cầu

            var command = new CreateBookingCommand(new BookingCreateDTO(
                UserId: userId,
                BookingDate: bookingDate,
                TotalPrice: totalPrice,
                Note: "Test booking",
                DepositAmount: tooLowDeposit,
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(courtId, startTime, endTime)
                }
            ));

            // Setup mock với minDepositPercentage = 50%
            SetupMocks(courtId, bookingDate, startTime, endTime, minDepositPercentage);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Should_RequireDeposit_WhenCourtRequiresMinimumDeposit()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(12);
            var minDepositPercentage = 50m; // 50% đặt cọc tối thiểu

            var command = new CreateBookingCommand(new BookingCreateDTO(
                UserId: userId,
                BookingDate: bookingDate,
                TotalPrice: 200m,
                Note: "Test booking",
                DepositAmount: 0, // Không đặt cọc
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(courtId, startTime, endTime)
                }
            ));

            // Setup mock
            SetupMocks(courtId, bookingDate, startTime, endTime, minDepositPercentage);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Should_AllowZeroDeposit_WhenCourtDoesNotRequireDeposit()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(12);
            var minDepositPercentage = 0m; // Không yêu cầu đặt cọc

            var command = new CreateBookingCommand(new BookingCreateDTO(
                UserId: userId,
                BookingDate: bookingDate,
                TotalPrice: 200m,
                Note: "Test booking",
                DepositAmount: 0, // Không đặt cọc
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(courtId, startTime, endTime)
                }
            ));

            // Setup mock
            SetupMocks(courtId, bookingDate, startTime, endTime, minDepositPercentage);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            _mockBookingRepository.Verify(r => r.AddBookingAsync(
                It.IsAny<Booking>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_Should_CalculateCorrectDeposit_WhenMultipleCourtDetailsWithDifferentRates()
        {
            // Arrange
            var courtId1 = Guid.NewGuid();
            var courtId2 = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingDate = DateTime.Today.AddDays(1);

            // Sân 1: minDepositPercentage = 30%, price = 100
            // Sân 2: minDepositPercentage = 50%, price = 150
            // Tổng giá: 250
            // Đặt cọc tối thiểu: 100*30% + 150*50% = 30 + 75 = 105
            var depositAmount = 105m;

            var command = new CreateBookingCommand(new BookingCreateDTO(
                UserId: userId,
                BookingDate: bookingDate,
                TotalPrice: 250m,
                Note: "Test booking",
                DepositAmount: depositAmount,
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(courtId1, TimeSpan.FromHours(10), TimeSpan.FromHours(11)),
                    new BookingDetailCreateDTO(courtId2, TimeSpan.FromHours(11), TimeSpan.FromHours(12))
                }
            ));

            // Setup mock court 1
            var mockCourt1 = new Court();
            typeof(Court).GetProperty("Id").SetValue(mockCourt1, CourtId.Of(courtId1));
            typeof(Court).GetProperty("MinDepositPercentage").SetValue(mockCourt1, 30m);
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                It.Is<CourtId>(id => id.Value == courtId1),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCourt1);

            // Setup mock court 2
            var mockCourt2 = new Court();
            typeof(Court).GetProperty("Id").SetValue(mockCourt2, CourtId.Of(courtId2));
            typeof(Court).GetProperty("MinDepositPercentage").SetValue(mockCourt2, 50m);
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                It.Is<CourtId>(id => id.Value == courtId2),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCourt2);

            // Setup mock schedules cho hai sân
            SetupMockSchedules(courtId1, bookingDate);
            SetupMockSchedules(courtId2, bookingDate);

            // Setup không có booking trước đó
            _mockBookingRepository.Setup(r => r.GetBookingsInDateRangeForCourtAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            _mockBookingRepository.Verify(r => r.AddBookingAsync(
                It.Is<Booking>(b => b.TotalPaid == depositAmount),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private void SetupMocks(Guid courtId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime, decimal minDepositPercentage = 0)
        {
            // Setup mock court
            var mockCourt = SetupMockCourt(courtId, minDepositPercentage);

            // Setup mock schedules
            SetupMockSchedules(courtId, bookingDate);

            // Setup không có booking trước đó
            _mockBookingRepository.Setup(r => r.GetBookingsInDateRangeForCourtAsync(
                courtId,
                bookingDate,
                bookingDate,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking>());
        }

        private Court SetupMockCourt(Guid courtId, decimal minDepositPercentage)
        {
            var mockCourt = new Court();
            typeof(Court).GetProperty("Id").SetValue(mockCourt, CourtId.Of(courtId));
            typeof(Court).GetProperty("MinDepositPercentage").SetValue(mockCourt, minDepositPercentage);

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                It.Is<CourtId>(id => id.Value == courtId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCourt);

            return mockCourt;
        }

        private void SetupMockSchedules(Guid courtId, DateTime bookingDate)
        {
            var dayOfWeek = (int)bookingDate.DayOfWeek + 1; // Chuyển đổi sang định dạng 1-7

            var mockSchedules = new List<CourtSchedule>
            {
                CourtSchedule.Create(
                    CourtScheduleId.Of(Guid.NewGuid()),
                    CourtId.Of(courtId),
                    new DayOfWeekValue(new int[] { dayOfWeek }),
                    TimeSpan.FromHours(8),
                    TimeSpan.FromHours(20),
                    100m
                )
            };

            _mockCourtScheduleRepository.Setup(r => r.GetSchedulesByCourtIdAsync(
                It.Is<CourtId>(id => id.Value == courtId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSchedules);
        }

        private Booking CreateExistingBooking(Guid courtId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime)
        {
            var booking = Booking.Create(
                BookingId.Of(Guid.NewGuid()),
                UserId.Of(Guid.NewGuid()),
                bookingDate,
                "Existing booking"
            );

            // Tạo schedules mock cho BookingDetail
            var schedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                CourtId.Of(courtId),
                new DayOfWeekValue(new int[] { 1, 2, 3, 4, 5, 6, 7 }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(20),
                100m
            );

            // Thêm chi tiết booking
            typeof(Booking).GetMethod("AddBookingDetail").Invoke(
                booking,
                new object[] {
                    CourtId.Of(courtId),
                    startTime,
                    endTime,
                    new List<CourtSchedule> { schedule }
                }
            );

            return booking;
        }
    }
}
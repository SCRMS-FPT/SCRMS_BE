using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using Xunit;
using CourtBooking.Domain.Exceptions;

namespace CourtBooking.Test.Domain.Models
{
    public class BookingDetailTests
    {
        [Fact]
        public void Create_Should_ReturnValidBookingDetail_WhenParametersAreValid()
        {
            // Arrange
            var bookingId = BookingId.Of(Guid.NewGuid());
            var courtId = CourtId.Of(Guid.NewGuid());
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(12);
            var schedules = CreateSampleSchedules(courtId);

            // Act
            var bookingDetail = BookingDetail.Create(bookingId, courtId, startTime, endTime, schedules);

            // Assert
            Assert.NotNull(bookingDetail);
            Assert.Equal(bookingId, bookingDetail.BookingId);
            Assert.Equal(courtId, bookingDetail.CourtId);
            Assert.Equal(startTime, bookingDetail.StartTime);
            Assert.Equal(endTime, bookingDetail.EndTime);
            Assert.Equal(200.0m, bookingDetail.TotalPrice); // 2 giờ * 100/giờ = 200
        }

        [Fact]
        public void Create_Should_ThrowDomainException_WhenStartTimeIsAfterEndTime()
        {
            // Arrange
            var bookingId = BookingId.Of(Guid.NewGuid());
            var courtId = CourtId.Of(Guid.NewGuid());
            var invalidStartTime = TimeSpan.FromHours(13);
            var invalidEndTime = TimeSpan.FromHours(10);
            var schedules = CreateSampleSchedules(courtId);

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() =>
                BookingDetail.Create(bookingId, courtId, invalidStartTime, invalidEndTime, schedules));

            Assert.Contains("Start time must be lower than end time", exception.Message);
        }

        [Fact]
        public void Create_Should_ThrowDomainException_WhenNoMatchingSchedule()
        {
            // Arrange
            var bookingId = BookingId.Of(Guid.NewGuid());
            var courtId = CourtId.Of(Guid.NewGuid());
            var startTime = TimeSpan.FromHours(6); // Không có lịch vào lúc 6h
            var endTime = TimeSpan.FromHours(7);
            var schedules = CreateSampleSchedules(courtId);

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() =>
                BookingDetail.Create(bookingId, courtId, startTime, endTime, schedules));

            Assert.Contains($"No schedule found for time slot starting at {startTime}", exception.Message);
        }

        [Fact]
        public void Create_Should_CalculateCorrectPrice_WhenBookingSpansDifferentPriceSlots()
        {
            // Arrange
            var bookingId = BookingId.Of(Guid.NewGuid());
            var courtId = CourtId.Of(Guid.NewGuid());
            var startTime = TimeSpan.FromHours(11); // Bắt đầu với giá 100
            var endTime = TimeSpan.FromHours(14);   // Kết thúc với giá 150
            var schedules = new List<CourtSchedule>
            {
                CreateSchedule(courtId, new int[] { 1, 2, 3 }, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 100.0m),
                CreateSchedule(courtId, new int[] { 1, 2, 3 }, TimeSpan.FromHours(12), TimeSpan.FromHours(18), 150.0m)
            };

            // Act
            var bookingDetail = BookingDetail.Create(bookingId, courtId, startTime, endTime, schedules);

            // Assert
            // 1 giờ với giá 100 + 2 giờ với giá 150 = 100 + 300 = 400
            Assert.Equal(400.0m, bookingDetail.TotalPrice);
        }

        private List<CourtSchedule> CreateSampleSchedules(CourtId courtId)
        {
            return new List<CourtSchedule>
            {
                CreateSchedule(courtId, new int[] { 1, 2, 3, 4, 5 }, TimeSpan.FromHours(8), TimeSpan.FromHours(18), 100.0m)
            };
        }

        private CourtSchedule CreateSchedule(CourtId courtId, int[] daysOfWeek, TimeSpan startTime, TimeSpan endTime, decimal priceSlot)
        {
            return CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                courtId,
                new DayOfWeekValue(daysOfWeek),
                startTime,
                endTime,
                priceSlot
            );
        }
    }
}
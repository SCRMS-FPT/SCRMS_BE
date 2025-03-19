using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using System;
using Xunit;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Exceptions;

namespace CourtBooking.Test.Domain.Models
{
    public class CourtScheduleTests
    {
        [Fact]
        public void Create_Should_ReturnValidCourtSchedule_WhenParametersAreValid()
        {
            // Arrange
            var id = CourtScheduleId.Of(Guid.NewGuid());
            var courtId = CourtId.Of(Guid.NewGuid());
            var dayOfWeek = new DayOfWeekValue(new int[] { 1, 2, 3 });
            var startTime = TimeSpan.FromHours(8);
            var endTime = TimeSpan.FromHours(12);
            var priceSlot = 100.0m;

            // Act
            var courtSchedule = CourtSchedule.Create(id, courtId, dayOfWeek, startTime, endTime, priceSlot);

            // Assert
            Assert.Equal(id, courtSchedule.Id);
            Assert.Equal(courtId, courtSchedule.CourtId);
            Assert.Equal(dayOfWeek, courtSchedule.DayOfWeek);
            Assert.Equal(startTime, courtSchedule.StartTime);
            Assert.Equal(endTime, courtSchedule.EndTime);
            Assert.Equal(priceSlot, courtSchedule.PriceSlot);
            Assert.Equal(CourtScheduleStatus.Available, courtSchedule.Status);
        }

        [Fact]
        public void Update_Should_UpdateCourtScheduleProperties_WhenParametersAreValid()
        {
            // Arrange
            var courtSchedule = CreateValidCourtSchedule();
            var newDayOfWeek = new DayOfWeekValue(new int[] { 4, 5, 6 });
            var newStartTime = TimeSpan.FromHours(9);
            var newEndTime = TimeSpan.FromHours(13);
            var newPriceSlot = 150.0m;
            var newStatus = CourtScheduleStatus.Maintenance;

            // Act
            courtSchedule.Update(newDayOfWeek, newStartTime, newEndTime, newPriceSlot, newStatus);

            // Assert
            Assert.Equal(newDayOfWeek, courtSchedule.DayOfWeek);
            Assert.Equal(newStartTime, courtSchedule.StartTime);
            Assert.Equal(newEndTime, courtSchedule.EndTime);
            Assert.Equal(newPriceSlot, courtSchedule.PriceSlot);
            Assert.Equal(newStatus, courtSchedule.Status);
            Assert.NotNull(courtSchedule.LastModified);
        }

        [Fact]
        public void Update_Should_ThrowDomainException_WhenStartTimeIsAfterEndTime()
        {
            // Arrange
            var courtSchedule = CreateValidCourtSchedule();
            var dayOfWeek = new DayOfWeekValue(new int[] { 1, 2, 3 });
            var invalidStartTime = TimeSpan.FromHours(14);
            var invalidEndTime = TimeSpan.FromHours(12);
            var priceSlot = 100.0m;

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() =>
                courtSchedule.Update(dayOfWeek, invalidStartTime, invalidEndTime, priceSlot, CourtScheduleStatus.Available));

            Assert.Contains("StartTime must be before EndTime", exception.Message);
        }

        [Fact]
        public void Update_Should_ThrowDomainException_WhenPriceIsNegative()
        {
            // Arrange
            var courtSchedule = CreateValidCourtSchedule();
            var dayOfWeek = new DayOfWeekValue(new int[] { 1, 2, 3 });
            var startTime = TimeSpan.FromHours(8);
            var endTime = TimeSpan.FromHours(12);
            var invalidPrice = -50.0m;

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() =>
                courtSchedule.Update(dayOfWeek, startTime, endTime, invalidPrice, CourtScheduleStatus.Available));

            Assert.Contains("Price must be non-negative", exception.Message);
        }

        private CourtSchedule CreateValidCourtSchedule()
        {
            return CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                CourtId.Of(Guid.NewGuid()),
                new DayOfWeekValue(new int[] { 1, 2, 3 }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(12),
                100.0m
            );
        }
    }
}
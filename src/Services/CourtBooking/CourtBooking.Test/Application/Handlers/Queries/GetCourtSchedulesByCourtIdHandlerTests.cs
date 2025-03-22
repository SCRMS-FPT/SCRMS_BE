using CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;
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

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetCourtSchedulesByCourtIdHandlerTests
    {
        private readonly Mock<ICourtScheduleRepository> _mockScheduleRepository;
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly GetCourtSchedulesByCourtIdHandler _handler;

        public GetCourtSchedulesByCourtIdHandlerTests()
        {
            _mockScheduleRepository = new Mock<ICourtScheduleRepository>();
            // Fix the constructor to include all required dependencies
            _mockCourtRepository = new Mock<ICourtRepository>();
            _handler = new GetCourtSchedulesByCourtIdHandler(
                _mockCourtRepository.Object,
                _mockScheduleRepository.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_When_NoSchedulesExist()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var query = new GetCourtSchedulesByCourtIdQuery(courtId);

            _mockScheduleRepository.Setup(r => r.GetSchedulesByCourt(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            // Fix the assertions for Handle_Should_ReturnEmptyList_When_NoSchedulesExist
            Assert.Empty(result.CourtSchedules);

            _mockScheduleRepository.Verify(r => r.GetSchedulesByCourt(
                CourtId.Of(courtId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnSchedulesList_When_SchedulesExist()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var query = new GetCourtSchedulesByCourtIdQuery(courtId);

            var schedule = CourtSchedule.Create(
                CourtScheduleId.Of(scheduleId),
                CourtId.Of(courtId),
                DayOfWeekValue.Of(new List<int> { 1, 2, 3, 4, 5 }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(22),
                30
            );

            _mockScheduleRepository.Setup(r => r.GetSchedulesByCourt(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule> { schedule });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            // Fix the assertions for Handle_Should_ReturnSchedulesList_When_SchedulesExist
            Assert.Single(result.CourtSchedules);
            var scheduleDto = result.CourtSchedules[0];
            Assert.Equal(scheduleId, scheduleDto.Id);
            Assert.Equal(courtId, scheduleDto.CourtId);
            Assert.Equal(new List<int> { 1, 2, 3, 4, 5 }, scheduleDto.DayOfWeek);
            Assert.Equal(TimeSpan.FromHours(8), scheduleDto.StartTime);
            Assert.Equal(TimeSpan.FromHours(22), scheduleDto.EndTime);
            Assert.Equal(30, scheduleDto.PriceSlot);
            Assert.Equal(0, scheduleDto.Status);

            _mockScheduleRepository.Verify(r => r.GetSchedulesByCourt(
                CourtId.Of(courtId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_MapScheduleStatus_When_SchedulesExist()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var query = new GetCourtSchedulesByCourtIdQuery(courtId);

            var activeSchedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                CourtId.Of(courtId),
                DayOfWeekValue.Of(new List<int> { 1, 2, 3 }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(12),
                30
            );

            var maintenanceSchedule = CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                CourtId.Of(courtId),
                DayOfWeekValue.Of(new List<int> { 4, 5 }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(12),
                30
            );

            // Set to maintenance
            maintenanceSchedule.Update(maintenanceSchedule.DayOfWeek, maintenanceSchedule.StartTime, maintenanceSchedule.EndTime, maintenanceSchedule.PriceSlot, CourtScheduleStatus.Maintenance);

            _mockScheduleRepository.Setup(r => r.GetSchedulesByCourt(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule> { activeSchedule, maintenanceSchedule });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            // Fix the assertions for Handle_Should_MapScheduleStatus_When_SchedulesExist
            Assert.Equal(2, result.CourtSchedules.Count);
            Assert.Equal(0, result.CourtSchedules[0].Status);
            Assert.Equal(2, result.CourtSchedules[1].Status);
        }
    }
}
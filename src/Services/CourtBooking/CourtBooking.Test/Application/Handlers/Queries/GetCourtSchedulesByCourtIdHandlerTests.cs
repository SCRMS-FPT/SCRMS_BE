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
            var courtId = Guid.Parse("9247f878-7c03-4c47-a4af-511ac7afb039");
            var query = new GetCourtSchedulesByCourtIdQuery(courtId);

            var court = CreateTestCourt(courtId);
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockScheduleRepository.Setup(r => r.GetSchedulesByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result.CourtSchedules);

            _mockScheduleRepository.Verify(r => r.GetSchedulesByCourtIdAsync(
                It.Is<CourtId>(id => id.Value == courtId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnSchedulesList_When_SchedulesExist()
        {
            // Arrange
            var courtId = Guid.Parse("6e550f3d-3d18-4470-81fd-0986b45832a9");
            var scheduleId = Guid.NewGuid();
            var query = new GetCourtSchedulesByCourtIdQuery(courtId);

            var court = CreateTestCourt(courtId);
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            var schedule = CourtSchedule.Create(
                CourtScheduleId.Of(scheduleId),
                CourtId.Of(courtId),
                DayOfWeekValue.Of(new List<int> { 1, 2, 3, 4, 5 }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(22),
                30
            );

            _mockScheduleRepository.Setup(r => r.GetSchedulesByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule> { schedule });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.CourtSchedules);
            var scheduleDto = result.CourtSchedules[0];
            Assert.Equal(scheduleId, scheduleDto.Id);
            Assert.Equal(courtId, scheduleDto.CourtId);
            Assert.Equal(new List<int> { 1, 2, 3, 4, 5 }, scheduleDto.DayOfWeek.ToList());
            Assert.Equal(TimeSpan.FromHours(8), scheduleDto.StartTime);
            Assert.Equal(TimeSpan.FromHours(22), scheduleDto.EndTime);
            Assert.Equal(30, scheduleDto.PriceSlot);
            Assert.Equal(0, scheduleDto.Status);

            _mockScheduleRepository.Verify(r => r.GetSchedulesByCourtIdAsync(
                It.Is<CourtId>(id => id.Value == courtId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_MapScheduleStatus_When_SchedulesExist()
        {
            // Arrange
            var courtId = Guid.Parse("4c202efb-e45c-482e-88bf-c207c975ec5d");
            var query = new GetCourtSchedulesByCourtIdQuery(courtId);

            var court = CreateTestCourt(courtId);
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

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

            _mockScheduleRepository.Setup(r => r.GetSchedulesByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtSchedule> { activeSchedule, maintenanceSchedule });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.CourtSchedules.Count);
            Assert.Equal((int)CourtScheduleStatus.Available, result.CourtSchedules[0].Status);
            Assert.Equal((int)CourtScheduleStatus.Maintenance, result.CourtSchedules[1].Status);
        }

        private Court CreateTestCourt(Guid courtId)
        {
            return Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test court description",
                null,
                CourtType.Indoor,
                50,
                24,
                100
            );
        }
    }
}
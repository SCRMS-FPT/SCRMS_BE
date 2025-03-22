using BuildingBlocks.Exceptions;
using CourtBooking.Application.CourtManagement.Command.CreateCourtSchedule;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Commands
{
    public class CreateCourtScheduleHandlerTests
    {
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly Mock<ICourtScheduleRepository> _mockCourtScheduleRepository;
        private readonly CreateCourtScheduleHandler _handler;

        public CreateCourtScheduleHandlerTests()
        {
            _mockCourtRepository = new Mock<ICourtRepository>();
            _mockCourtScheduleRepository = new Mock<ICourtScheduleRepository>();

            _handler = new CreateCourtScheduleHandler(
                _mockCourtRepository.Object,
                _mockCourtScheduleRepository.Object
            );
        }

        [Fact]
        public async Task Handle_Should_CreateSchedule_When_Valid()
        {
            // Arrange
            var courtId = Guid.NewGuid();

            // Tạo command đúng với định nghĩa trong CreateCourtScheduleCommand.cs
            var command = new CreateCourtScheduleCommand(
                courtId,
                new int[] { 1, 2, 3, 4, 5 }, // Monday to Friday
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(22),
                30.0m
            );

            // Setup court để trả về khi gọi GetCourtByIdAsync
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Tennis court description",
                "Facilities",
                CourtType.Indoor,
                50m
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(CourtId.Of(courtId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Setup schedule repository để bắt lại schedule được thêm
            CourtSchedule addedSchedule = null;
            _mockCourtScheduleRepository.Setup(r => r.AddCourtScheduleAsync(It.IsAny<CourtSchedule>(), It.IsAny<CancellationToken>()))
                .Callback<CourtSchedule, CancellationToken>((schedule, _) => addedSchedule = schedule)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);

            // Verify repository calls
            _mockCourtScheduleRepository.Verify(r => r.AddCourtScheduleAsync(It.IsAny<CourtSchedule>(), It.IsAny<CancellationToken>()), Times.Once);

            // Verify schedule properties
            Assert.NotNull(addedSchedule);
            Assert.Equal(CourtId.Of(courtId), addedSchedule.CourtId);
            Assert.Equal(TimeSpan.FromHours(8), addedSchedule.StartTime);
            Assert.Equal(TimeSpan.FromHours(22), addedSchedule.EndTime);
            Assert.Equal(30.0m, addedSchedule.PriceSlot);
            Assert.Contains(1, addedSchedule.DayOfWeek.Days);
            Assert.Contains(5, addedSchedule.DayOfWeek.Days);
        }

        [Fact]
        public async Task Handle_Should_ThrowNotFoundException_When_CourtNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();

            var command = new CreateCourtScheduleCommand(
                courtId,
                new int[] { 1, 2, 3, 4, 5 },
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(22),
                30.0m
            );

            // Setup để trả về null khi gọi GetCourtByIdAsync
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(CourtId.Of(courtId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Court)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            // Verify repository calls - không gọi AddCourtScheduleAsync khi court không tồn tại
            _mockCourtScheduleRepository.Verify(r => r.AddCourtScheduleAsync(It.IsAny<CourtSchedule>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_CreateScheduleWithCorrectData()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();

            var command = new CreateCourtScheduleCommand(
                courtId,
                new int[] { 1, 3, 5 }, // Monday, Wednesday, Friday
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(17),
                50.0m
            );

            // Setup court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Tennis court description",
                "Facilities",
                CourtType.Indoor,
                50m
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(CourtId.Of(courtId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Setup để lưu schedule ID khi tạo mới
            Guid capturedScheduleId = Guid.Empty;
            _mockCourtScheduleRepository.Setup(r => r.AddCourtScheduleAsync(It.IsAny<CourtSchedule>(), It.IsAny<CancellationToken>()))
                .Callback<CourtSchedule, CancellationToken>((schedule, _) => capturedScheduleId = schedule.Id.Value)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(capturedScheduleId, result.Id);

            // Verify repository calls
            _mockCourtScheduleRepository.Verify(r => r.AddCourtScheduleAsync(
                It.Is<CourtSchedule>(s =>
                    s.CourtId == CourtId.Of(courtId) &&
                    s.StartTime == TimeSpan.FromHours(9) &&
                    s.EndTime == TimeSpan.FromHours(17) &&
                    s.PriceSlot == 50.0m &&
                    s.DayOfWeek.Days.Contains(1) &&
                    s.DayOfWeek.Days.Contains(3) &&
                    s.DayOfWeek.Days.Contains(5)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        }
    }
}
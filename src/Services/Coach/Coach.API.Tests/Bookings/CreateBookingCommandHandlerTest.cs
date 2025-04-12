using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Coach.API.Data.Repositories;
using Coach.API.Data;
using Moq;
using Xunit;
using System.Runtime.CompilerServices;
using Coach.API.Data.Models;
using Coach.API.Features.Bookings.CreateBooking;

[assembly: InternalsVisibleTo("Coach.API.Tests")]

namespace Coach.API.Tests.Bookings
{
    public class CreateBookingCommandHandlerTests
    {
        // Test 1: Tạo booking hợp lệ (Normal)
        [Fact]
        public async Task Handle_ValidBooking_CreatesBooking()
        {
            // Arrange
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(today), TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), null);
            var coach = new Data.Models.Coach { UserId = command.CoachId, RatePerHour = 50 };
            var schedule = new CoachSchedule { DayOfWeek = dayOfWeek, StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(17)) };

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(repo => repo.GetCoachByIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(coach);

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(repo => repo.GetCoachSchedulesByCoachIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule> { schedule });

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.HasOverlappingCoachBookingAsync(command.CoachId, command.BookingDate, command.StartTime, command.EndTime, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            var mockContext = new Mock<CoachDbContext>();
            var mockMediator = new Mock<IMediator>();

            var handler = new CreateBookingCommandHandler(mockCoachRepo.Object, mockScheduleRepo.Object, mockBookingRepo.Object, mockPackageRepo.Object, mockContext.Object, mockMediator.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockBookingRepo.Verify(repo => repo.AddCoachBookingAsync(It.IsAny<CoachBooking>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(m => m.Publish(It.IsAny<BookingCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(0, result.SessionsRemaining);
        }

        // Test 2: Coach không tồn tại (Abnormal)
        [Fact]
        public async Task Handle_CoachNotFound_ThrowsException()
        {
            // Arrange
            var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), null);

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(repo => repo.GetCoachByIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync((Data.Models.Coach)null);

            var handler = new CreateBookingCommandHandler(mockCoachRepo.Object, null, null, null, null, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Coach not found", exception.Message);
        }

        // Test 3: Thời gian booking không hợp lệ (Abnormal)
        [Fact]
        public void Validate_InvalidTime_ValidationFails()
        {
            // Arrange
            var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), null);
            var validator = new CreateBookingCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Start time must be earlier than end time.");
        }

        // Test 4: Thời gian booking ngoài lịch trình coach (Abnormal)
        [Fact]
        public async Task Handle_TimeOutsideSchedule_ThrowsException()
        {
            // Arrange
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(today), TimeOnly.FromTimeSpan(TimeSpan.FromHours(18)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(19)), null);
            var coach = new Data.Models.Coach { UserId = command.CoachId };
            var schedule = new CoachSchedule { DayOfWeek = dayOfWeek, StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(17)) };

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(repo => repo.GetCoachByIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(coach);

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(repo => repo.GetCoachSchedulesByCoachIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule> { schedule });

            var handler = new CreateBookingCommandHandler(mockCoachRepo.Object, mockScheduleRepo.Object, null, null, null, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Booking time is outside coach's available hours", exception.Message);
        }

        // Test 5: Có booking chồng lấp (Abnormal)
        [Fact]
        public async Task Handle_OverlappingBooking_ThrowsException()
        {
            // Arrange
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(today), TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), null);
            var coach = new Data.Models.Coach { UserId = command.CoachId };
            var schedule = new CoachSchedule { DayOfWeek = dayOfWeek, StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(17)) };

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(repo => repo.GetCoachByIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(coach);

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(repo => repo.GetCoachSchedulesByCoachIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule> { schedule });

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.HasOverlappingCoachBookingAsync(command.CoachId, command.BookingDate, command.StartTime, command.EndTime, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var handler = new CreateBookingCommandHandler(mockCoachRepo.Object, mockScheduleRepo.Object, mockBookingRepo.Object, null, null, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("The selected time slot is already booked", exception.Message);
        }

        // Test 6: Gói dịch vụ không tồn tại (Abnormal)
        [Fact]
        public async Task Handle_PackageNotFound_ThrowsException()
        {
            // Arrange
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            var packageId = Guid.NewGuid();
            var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(today), TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)), packageId);
            var coach = new Data.Models.Coach { UserId = command.CoachId };
            var schedule = new CoachSchedule { DayOfWeek = dayOfWeek, StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(17)) };

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(repo => repo.GetCoachByIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(coach);

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(repo => repo.GetCoachSchedulesByCoachIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule> { schedule });

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.HasOverlappingCoachBookingAsync(command.CoachId, command.BookingDate, command.StartTime, command.EndTime, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(repo => repo.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync((CoachPackage)null);

            var handler = new CreateBookingCommandHandler(mockCoachRepo.Object, mockScheduleRepo.Object, mockBookingRepo.Object, mockPackageRepo.Object, null, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Package not found", exception.Message);
        }

        // Test 7: Booking với thời gian giáp ranh (Boundary)
        [Fact]
        public async Task Handle_BoundaryTime_CreatesBooking()
        {
            // Arrange
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(today), TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)), null);
            var coach = new Data.Models.Coach { UserId = command.CoachId, RatePerHour = 50 };
            var schedule = new CoachSchedule { DayOfWeek = dayOfWeek, StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(17)) };

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(repo => repo.GetCoachByIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(coach);

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(repo => repo.GetCoachSchedulesByCoachIdAsync(command.CoachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule> { schedule });

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(repo => repo.HasOverlappingCoachBookingAsync(command.CoachId, command.BookingDate, command.StartTime, command.EndTime, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            var mockContext = new Mock<CoachDbContext>();
            var mockMediator = new Mock<IMediator>();

            var handler = new CreateBookingCommandHandler(mockCoachRepo.Object, mockScheduleRepo.Object, mockBookingRepo.Object, mockPackageRepo.Object, mockContext.Object, mockMediator.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockBookingRepo.Verify(repo => repo.AddCoachBookingAsync(It.IsAny<CoachBooking>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(m => m.Publish(It.IsAny<BookingCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
        }
    }
}
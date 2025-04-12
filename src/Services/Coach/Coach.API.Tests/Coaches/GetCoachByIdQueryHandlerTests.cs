using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Coach.API.Exceptions;
using Coach.API.Data.Repositories;
using Moq;
using Xunit;
using Coach.API.Data.Models;
using Coach.API.Features.Coaches.GetCoachById;
using BuildingBlocks.Exceptions;

namespace Coach.API.Tests.Coaches
{
    public class GetCoachByIdQueryHandlerTests
    {
        // Test 1: Lấy coach hợp lệ (Normal)
        [Fact]
        public async Task Handle_ExistingCoach_ReturnsCoachResponse()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetCoachByIdQuery(coachId);
            var coach = new Data.Models.Coach { UserId = coachId, Bio = "Test", RatePerHour = 50m, CreatedAt = DateTime.UtcNow };
            var sport = new CoachSport { CoachId = coachId, SportId = Guid.NewGuid() };
            var package = new CoachPackage { Id = Guid.NewGuid(), Name = "Package", Description = "Test", Price = 100m, SessionCount = 5 };
            var schedule = new CoachSchedule { Id = Guid.NewGuid(), CoachId = coachId, DayOfWeek = 1, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) };

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(coach);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.GetCoachSportsByCoachIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSport> { sport });

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachPackage> { package });

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule> { schedule });

            var handler = new GetCoachByIdQueryHandler(
                mockCoachRepo.Object,
                mockSportRepo.Object,
                mockPackageRepo.Object,
                mockScheduleRepo.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(coachId, result.Id);
            Assert.Equal(coach.Bio, result.Bio);
            Assert.Equal(1, result.SportIds.Count);
            Assert.Equal(1, result.Packages.Count);
            Assert.Equal(1, result.WeeklySchedule.Count);
        }

        // Test 2: Coach không tồn tại (Abnormal)
        [Fact]
        public async Task Handle_NonExistingCoach_ThrowsNotFoundException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetCoachByIdQuery(coachId);
            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync((Data.Models.Coach)null);

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();

            var handler = new GetCoachByIdQueryHandler(
                mockCoachRepo.Object,
                Mock.Of<ICoachSportRepository>(),
                Mock.Of<ICoachPackageRepository>(),
                mockScheduleRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<CoachNotFoundException>(() => handler.Handle(query, CancellationToken.None));
        }

        // Test 3: Id rỗng (Abnormal)
        [Fact]
        public void Validate_EmptyId_ValidationFails()
        {
            // Arrange
            var query = new GetCoachByIdQuery(Guid.Empty);
            var validator = new GetCoachByIdQueryValidator();

            // Act
            var result = validator.Validate(query);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Id is required");
        }

        // Test 4: Không có sport hoặc package (Boundary)
        [Fact]
        public async Task Handle_NoSportsOrPackages_ReturnsEmptyLists()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetCoachByIdQuery(coachId);
            var coach = new Data.Models.Coach { UserId = coachId, Bio = "Test", RatePerHour = 50m, CreatedAt = DateTime.UtcNow };

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(coach);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.GetCoachSportsByCoachIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSport>());

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachPackage>());

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule>());

            var handler = new GetCoachByIdQueryHandler(
                mockCoachRepo.Object,
                mockSportRepo.Object,
                mockPackageRepo.Object,
                mockScheduleRepo.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.SportIds);
            Assert.Empty(result.Packages);
            Assert.Empty(result.WeeklySchedule);
        }
    }
}
using Coach.API.Data.Models;
using Coach.API.Data;
using Coach.API.Data.Repositories;
using Coach.API.Exceptions;
using Coach.API.Features.Coaches.GetCoaches;
using Coach.API.Features.Coaches.GetMyCoachProfile;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Coach.API.Tests.Coaches
{
    // Define the WeeklySchedule class to match the CoachResponse expected type
    public class WeeklySchedule : List<CoachWeeklyScheduleResponse>
    { }

    public class GetMyCoachProfileQueryHandlerTests
    {
        private readonly Mock<ICoachRepository> _mockCoachRepository;
        private readonly Mock<ICoachSportRepository> _mockSportRepository;
        private readonly Mock<ICoachPackageRepository> _mockPackageRepository;
        private readonly Mock<ICoachScheduleRepository> _mockScheduleRepository;
        private readonly GetMyCoachProfileQueryHandler _handler;

        public GetMyCoachProfileQueryHandlerTests()
        {
            _mockCoachRepository = new Mock<ICoachRepository>();
            _mockSportRepository = new Mock<ICoachSportRepository>();
            _mockPackageRepository = new Mock<ICoachPackageRepository>();
            _mockScheduleRepository = new Mock<ICoachScheduleRepository>();
            _handler = new GetMyCoachProfileQueryHandler(
                _mockCoachRepository.Object,
                _mockSportRepository.Object,
                _mockPackageRepository.Object,
                _mockScheduleRepository.Object);
        }

        [Fact]
        public async Task Handle_ValidQuery_ReturnsCoachProfile()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetMyCoachProfileQuery(coachId);

            var coach = new Data.Models.Coach
            {
                UserId = coachId,
                FullName = "Test Coach",
                Email = "coach@test.com",
                Phone = "1234567890",
                Avatar = "http://test.com/avatar.jpg",
                Bio = "Test bio",
                RatePerHour = 50.0m,
                ImageUrls = "img1.jpg|img2.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var sports = new List<CoachSport>
            {
                new CoachSport { CoachId = coachId, SportId = Guid.NewGuid() },
                new CoachSport { CoachId = coachId, SportId = Guid.NewGuid() }
            };

            var packages = new List<CoachPackage>
            {
                new CoachPackage { Id = Guid.NewGuid(), CoachId = coachId, Name = "Package 1", Description = "Description 1", Price = 100.0m, SessionCount = 5 }
            };

            var schedules = new List<CoachSchedule>
            {
                new CoachSchedule {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    DayOfWeek = 2, // Monday (0 = Sunday, 1 = Monday theo enum DayOfWeek nhưng 1 = Sunday, 2 = Monday theo model)
                    StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)),
                    EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(12))
                }
            };

            _mockCoachRepository.Setup(x => x.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coach);
            _mockSportRepository.Setup(x => x.GetCoachSportsByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(sports);
            _mockPackageRepository.Setup(x => x.GetCoachPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(packages);
            _mockScheduleRepository.Setup(x => x.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(schedules);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(coachId, result.Id);
            Assert.Equal(coach.FullName, result.FullName);
            Assert.Equal(coach.Email, result.Email);
            Assert.Equal(coach.Phone, result.Phone);
            Assert.Equal(coach.Avatar, result.Avatar);
            Assert.Equal(coach.Bio, result.Bio);
            Assert.Equal(coach.RatePerHour, result.RatePerHour);
            Assert.Equal(2, result.ImageUrls.Count);
            Assert.Equal(2, result.SportIds.Count);
            Assert.Equal(1, result.Packages.Count);
            Assert.NotNull(result.WeeklySchedule);

            // Check Monday schedule
            var mondaySchedule = result.WeeklySchedule.FirstOrDefault(s => s.DayName == "Monday");
            Assert.NotNull(mondaySchedule);
        }

        [Fact]
        public async Task Handle_CoachNotFound_ThrowsCoachNotFoundException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetMyCoachProfileQuery(coachId);

            _mockCoachRepository.Setup(x => x.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Models.Coach)null);

            // Act & Assert
            await Assert.ThrowsAsync<CoachNotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithEmptySports_ReturnsEmptySportIds()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetMyCoachProfileQuery(coachId);

            var coach = new Data.Models.Coach
            {
                UserId = coachId,
                FullName = "Test Coach",
                Email = "coach@test.com",
                Phone = "1234567890",
                Avatar = "http://test.com/avatar.jpg",
                Bio = "Test bio",
                RatePerHour = 50.0m,
                CreatedAt = DateTime.UtcNow
            };

            _mockCoachRepository.Setup(x => x.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coach);
            _mockSportRepository.Setup(x => x.GetCoachSportsByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSport>());
            _mockPackageRepository.Setup(x => x.GetCoachPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachPackage>());
            _mockScheduleRepository.Setup(x => x.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSchedule>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.SportIds);
            Assert.Empty(result.Packages);
            Assert.NotNull(result.WeeklySchedule);
        }

        [Fact]
        public async Task Handle_WithNullImageUrls_ReturnsEmptyImageList()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetMyCoachProfileQuery(coachId);

            var coach = new Data.Models.Coach
            {
                UserId = coachId,
                FullName = "Test Coach",
                Email = "coach@test.com",
                Phone = "1234567890",
                Avatar = "http://test.com/avatar.jpg",
                Bio = "Test bio",
                RatePerHour = 50.0m,
                ImageUrls = null, // null image urls
                CreatedAt = DateTime.UtcNow
            };

            _mockCoachRepository.Setup(x => x.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coach);
            _mockSportRepository.Setup(x => x.GetCoachSportsByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSport>());
            _mockPackageRepository.Setup(x => x.GetCoachPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachPackage>());
            _mockScheduleRepository.Setup(x => x.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSchedule>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ImageUrls);
            Assert.Empty(result.ImageUrls);
        }

        [Fact]
        public async Task Handle_WithEmptyImageUrls_ReturnsEmptyImageList()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetMyCoachProfileQuery(coachId);

            var coach = new Data.Models.Coach
            {
                UserId = coachId,
                FullName = "Test Coach",
                Email = "coach@test.com",
                Phone = "1234567890",
                Avatar = "http://test.com/avatar.jpg",
                Bio = "Test bio",
                RatePerHour = 50.0m,
                ImageUrls = "", // empty image urls
                CreatedAt = DateTime.UtcNow
            };

            _mockCoachRepository.Setup(x => x.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coach);
            _mockSportRepository.Setup(x => x.GetCoachSportsByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSport>());
            _mockPackageRepository.Setup(x => x.GetCoachPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachPackage>());
            _mockScheduleRepository.Setup(x => x.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSchedule>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ImageUrls);
            Assert.Empty(result.ImageUrls);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Coach.API.Data.Repositories;
using Moq;
using Xunit;
using Coach.API.Data.Models;
using Coach.API.Features.Coaches.GetCoaches;

namespace Coach.API.Tests.Coaches
{
    public class GetCoachesQueryHandlerTests
    {
        // Test 1: Lấy danh sách coach hợp lệ (Normal)
        [Fact]
        public async Task Handle_ExistingCoaches_ReturnsCoachList()
        {
            // Arrange
            var coach1 = new Data.Models.Coach { UserId = Guid.NewGuid(), Bio = "Coach 1", RatePerHour = 50m, CreatedAt = DateTime.UtcNow };
            var coach2 = new Data.Models.Coach { UserId = Guid.NewGuid(), Bio = "Coach 2", RatePerHour = 60m, CreatedAt = DateTime.UtcNow };
            var coaches = new List<Data.Models.Coach> { coach1, coach2 };

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetAllCoachesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(coaches);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.GetCoachSportsByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSport> { new CoachSport { SportId = Guid.NewGuid() } });

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackagesByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachPackage> { new CoachPackage { Id = Guid.NewGuid(), Name = "Package" } });

            var handler = new GetCoachesQueryHandler(mockCoachRepo.Object, mockSportRepo.Object, mockPackageRepo.Object);

            // Act
            var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

            // Assert
            var responses = result.ToList();
            Assert.Equal(2, responses.Count());
            Assert.Equal(coach1.UserId, responses[0].UserId);
            Assert.Equal(coach2.UserId, responses[1].UserId);
            Assert.Equal(1, responses[0].SportIds.Count);
            Assert.Equal(1, responses[0].Packages.Count);
        }

        // Test 2: Không có coach nào (Boundary)
        [Fact]
        public async Task Handle_NoCoaches_ReturnsEmptyList()
        {
            // Arrange
            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetAllCoachesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Data.Models.Coach>());

            var handler = new GetCoachesQueryHandler(mockCoachRepo.Object, null, null);

            // Act
            var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        // Test 3: Một coach không có sport hoặc package (Boundary)
        [Fact]
        public async Task Handle_CoachWithNoSportsOrPackages_ReturnsEmptyLists()
        {
            // Arrange
            var coach = new Data.Models.Coach { UserId = Guid.NewGuid(), Bio = "Test", RatePerHour = 50m, CreatedAt = DateTime.UtcNow };
            var coaches = new List<Data.Models.Coach> { coach };

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetAllCoachesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(coaches);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.GetCoachSportsByCoachIdAsync(coach.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSport>());

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackagesByCoachIdAsync(coach.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachPackage>());

            var handler = new GetCoachesQueryHandler(mockCoachRepo.Object, mockSportRepo.Object, mockPackageRepo.Object);

            // Act
            var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

            // Assert
            var responses = result.ToList();
            Assert.Single(responses);
            Assert.Empty(responses[0].SportIds);
            Assert.Empty(responses[0].Packages);
        }
    }
}
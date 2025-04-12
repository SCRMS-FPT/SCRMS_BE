﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Pagination;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Coaches.GetCoaches;
using Moq;
using Xunit;

namespace Coach.API.Tests.Coaches
{
    public class GetCoachesQueryHandlerTests
    {
        private List<Data.Models.Coach> GetSampleCoaches()
        {
            return new List<Data.Models.Coach>
            {
                new Data.Models.Coach
                {
                    UserId = Guid.NewGuid(),
                    FullName = "John Smith",
                    Email = "john@example.com",
                    Phone = "1234567890",
                    Avatar = "avatar1.jpg",
                    ImageUrls = "image1.jpg,image2.jpg",
                    Bio = "Tennis coach with 10 years experience",
                    RatePerHour = 50m,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    Status = "active"
                },
                new Data.Models.Coach
                {
                    UserId = Guid.NewGuid(),
                    FullName = "Jane Doe",
                    Email = "jane@example.com",
                    Phone = "0987654321",
                    Avatar = "avatar2.jpg",
                    ImageUrls = "image3.jpg,image4.jpg",
                    Bio = "Swimming coach with 5 years experience",
                    RatePerHour = 40m,
                    CreatedAt = DateTime.Now.AddDays(-15),
                    Status = "active"
                },
                new Data.Models.Coach
                {
                    UserId = Guid.NewGuid(),
                    FullName = "Robert Johnson",
                    Email = "robert@example.com",
                    Phone = "5556667777",
                    Avatar = "avatar3.jpg",
                    ImageUrls = "image5.jpg,image6.jpg",
                    Bio = "Basketball coach with 8 years experience",
                    RatePerHour = 60m,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    Status = "active"
                }
            };
        }

        // Normal cases
        [Fact]
        public async Task Handle_NoFilters_ReturnsAllActiveCoaches()
        {
            // Arrange
            var sampleCoaches = GetSampleCoaches();

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetAllCoachesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sampleCoaches);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.GetCoachSportsByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSport>());

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackagesByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachPackage>());

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSchedule>());

            var handler = new GetCoachesQueryHandler(
                mockCoachRepo.Object,
                mockSportRepo.Object,
                mockPackageRepo.Object,
                mockScheduleRepo.Object);

            var query = new GetCoachesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(0, result.PageIndex);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public async Task Handle_NameFilter_ReturnsMatchingCoaches()
        {
            // Arrange
            var sampleCoaches = GetSampleCoaches();

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetAllCoachesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sampleCoaches);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.GetCoachSportsByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSport>());

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackagesByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachPackage>());

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSchedule>());

            var handler = new GetCoachesQueryHandler(
                mockCoachRepo.Object,
                mockSportRepo.Object,
                mockPackageRepo.Object,
                mockScheduleRepo.Object);

            var query = new GetCoachesQuery(Name: "John");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Single(result.Data);
            Assert.Equal("John Smith", result.Data.First().FullName);
        }

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

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule>());

            var handler = new GetCoachesQueryHandler(mockCoachRepo.Object, mockSportRepo.Object, mockPackageRepo.Object, mockScheduleRepo.Object);

            // Act
            var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

            // Assert
            var responses = result.Data.ToList();
            Assert.Equal(2, responses.Count());
            Assert.Equal(coach1.UserId, responses[0].Id);
            Assert.Equal(coach2.UserId, responses[1].Id);
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

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule>());

            var handler = new GetCoachesQueryHandler(mockCoachRepo.Object, Mock.Of<ICoachSportRepository>(), Mock.Of<ICoachPackageRepository>(), mockScheduleRepo.Object);

            // Act
            var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result.Data);
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

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(coach.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CoachSchedule>());

            var handler = new GetCoachesQueryHandler(mockCoachRepo.Object, mockSportRepo.Object, mockPackageRepo.Object, mockScheduleRepo.Object);

            // Act
            var result = await handler.Handle(new GetCoachesQuery(), CancellationToken.None);

            // Assert
            var responses = result.Data.ToList();
            Assert.Single(responses);
            Assert.Empty(responses[0].SportIds);
            Assert.Empty(responses[0].Packages);
        }
    }
}
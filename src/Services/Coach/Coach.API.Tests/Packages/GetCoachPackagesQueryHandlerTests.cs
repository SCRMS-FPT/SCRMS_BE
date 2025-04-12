using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Packages.GetCoachPackages;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Coach.API.Tests.Packages
{
    public class GetCoachPackagesQueryHandlerTests
    {
        private readonly Mock<ICoachPackageRepository> _mockPackageRepository;
        private readonly GetCoachPackagesQueryHandler _handler;

        public GetCoachPackagesQueryHandlerTests()
        {
            _mockPackageRepository = new Mock<ICoachPackageRepository>();
            _handler = new GetCoachPackagesQueryHandler(_mockPackageRepository.Object);
        }

        [Fact]
        public async Task Handle_ValidQuery_ReturnsPackages()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetCoachPackagesQuery(coachId);
            
            var packages = new List<CoachPackage>
            {
                new CoachPackage
                {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    Name = "Package 1",
                    Description = "Description 1",
                    Price = 100.0m,
                    SessionCount = 5,
                    Status = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new CoachPackage
                {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    Name = "Package 2",
                    Description = "Description 2",
                    Price = 200.0m,
                    SessionCount = 10,
                    Status = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4)
                }
            };

            _mockPackageRepository.Setup(x => x.GetAllPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(packages);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Package 1", result[0].Name);
            Assert.Equal("Package 2", result[1].Name);
            Assert.Equal(100.0m, result[0].Price);
            Assert.Equal(200.0m, result[1].Price);
            Assert.Equal(5, result[0].SessionCount);
            Assert.Equal(10, result[1].SessionCount);
        }

        [Fact]
        public async Task Handle_EmptyPackages_ReturnsEmptyList()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetCoachPackagesQuery(coachId);
            
            _mockPackageRepository.Setup(x => x.GetAllPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachPackage>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetCoachPackagesQuery(coachId);
            
            _mockPackageRepository.Setup(x => x.GetAllPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Database error", new Exception()));

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => 
                _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_NullReturnFromRepository_HandlesGracefully()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetCoachPackagesQuery(coachId);
            
            _mockPackageRepository.Setup(x => x.GetAllPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<CoachPackage>)null);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => 
                _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_LargeNumberOfPackages_HandlesCorrectly()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var query = new GetCoachPackagesQuery(coachId);
            
            var packages = Enumerable.Range(1, 100).Select(i => 
                new CoachPackage
                {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    Name = $"Package {i}",
                    Description = $"Description {i}",
                    Price = i * 10.0m,
                    SessionCount = i,
                    Status = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i/2)
                }
            ).ToList();

            _mockPackageRepository.Setup(x => x.GetAllPackagesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(packages);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Count);
        }
    }
} 
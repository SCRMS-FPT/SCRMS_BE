using Coach.API.Data.Repositories;
using Coach.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Coach.API.Data.Models;

namespace Coach.API.Tests.Repositories
{
    public class CoachPackageRepositoryTests
    {
        private readonly CoachPackageRepository _repository;
        private readonly CoachDbContext _context;

        public CoachPackageRepositoryTests()
        {
            // Sử dụng cơ sở dữ liệu trong bộ nhớ
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Tạo cơ sở dữ liệu trong bộ nhớ
                          .Options;
            _context = new CoachDbContext(options);
            _repository = new CoachPackageRepository(_context);
        }

        [Fact]
        public async Task AddCoachPackageAsync_ValidPackage_AddsPackage()
        {
            // Arrange
            var package = new CoachPackage
            {
                Id = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                Name = "Test Package",
                Description = "Test Description",
                Price = 100.00m,
                SessionCount = 5
            };

            // Act
            await _repository.AddCoachPackageAsync(package, CancellationToken.None);

            // Assert
            var addedPackage = await _context.CoachPackages.FindAsync(package.Id);
            Assert.NotNull(addedPackage);
            Assert.Equal(package.Id, addedPackage.Id);
        }

        [Fact]
        public async Task AddCoachPackageAsync_InvalidCoachIdPackage_ThrowsException()
        {
            // Arrange
            var package = new CoachPackage { Id = Guid.NewGuid() }; // Missing CoachId, Name

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddCoachPackageAsync(package, CancellationToken.None));

            // Verify the exception message
            Assert.Contains("CoachId is required", exception.Message);
            Assert.Contains("Name is required", exception.Message);
        }

        [Fact]
        public async Task AddCoachPackageAsync_InvalidNamePackage_ThrowsException()
        {
            // Arrange
            var package = new CoachPackage { Id = Guid.NewGuid(), CoachId = Guid.NewGuid() }; // Missing CoachId, Name

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddCoachPackageAsync(package, CancellationToken.None));

            // Verify the exception message
            Assert.Contains("Name is required", exception.Message);
        }

        [Fact]
        public async Task GetCoachPackageByIdAsync_ExistingId_ReturnsPackage()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var package = new CoachPackage { Id = packageId, CoachId = Guid.NewGuid() };
            await _context.CoachPackages.AddAsync(package);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachPackageByIdAsync(packageId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageId, result.Id);
        }

        [Fact]
        public async Task GetCoachPackageByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachPackageByIdAsync(packageId, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCoachPackageAsync_ExistingPackage_UpdatesPackage()
        {
            // Arrange
            var package = new CoachPackage
            {
                Id = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                Name = "Updated Package",
                Price = 150.00m
            };
            await _context.CoachPackages.AddAsync(package);
            await _context.SaveChangesAsync();

            // Thay đổi thông tin package
            package.Name = "Updated Name";
            package.Price = 200.00m;

            // Act
            await _repository.UpdateCoachPackageAsync(package, CancellationToken.None);

            // Assert
            var updatedPackage = await _context.CoachPackages.FindAsync(package.Id);
            Assert.NotNull(updatedPackage);
            Assert.Equal("Updated Name", updatedPackage.Name);
            Assert.Equal(200.00m, updatedPackage.Price);
        }

        [Fact]
        public async Task UpdateCoachPackageAsync_NonExistingPackage_ThrowsException()
        {
            // Arrange
            var package = new CoachPackage { Id = Guid.NewGuid(), CoachId = Guid.NewGuid() };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateCoachPackageAsync(package, CancellationToken.None));

            // Optionally, verify the exception message
            Assert.Contains("The coach package does not exist", exception.Message);
        }

        [Fact]
        public async Task GetCoachPackagesByCoachIdAsync_CoachWithPackages_ReturnsPackages()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var packages = new List<CoachPackage>
            {
                new CoachPackage { Id = Guid.NewGuid(), CoachId = coachId },
                new CoachPackage { Id = Guid.NewGuid(), CoachId = coachId }
            };
            await _context.CoachPackages.AddRangeAsync(packages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachPackagesByCoachIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(coachId, p.CoachId));
        }

        [Fact]
        public async Task GetCoachPackagesByCoachIdAsync_CoachWithoutPackages_ReturnsEmptyList()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachPackagesByCoachIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }
}
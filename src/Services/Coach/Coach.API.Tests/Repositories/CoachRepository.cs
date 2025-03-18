using Coach.API.Data.Repositories;
using Coach.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coach.API.Tests.Repositories
{
    public class CoachRepositoryTests
    {
        private readonly CoachRepository _repository;
        private readonly CoachDbContext _context;

        public CoachRepositoryTests()
        {
            // Sử dụng cơ sở dữ liệu trong bộ nhớ
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Tạo cơ sở dữ liệu trong bộ nhớ
                          .Options;
            _context = new CoachDbContext(options);
            _repository = new CoachRepository(_context);
        }

        [Fact]
        public async Task AddCoachAsync_ValidCoach_AddsCoach()
        {
            // Arrange
            var coach = new Data.Models.Coach
            {
                UserId = Guid.NewGuid(),
                Bio = "Test Bio",
                RatePerHour = 50.00m
            };

            // Act
            await _repository.AddCoachAsync(coach, CancellationToken.None);

            // Assert
            var addedCoach = await _context.Coaches.FindAsync(coach.UserId);
            Assert.NotNull(addedCoach);
            Assert.Equal(coach.UserId, addedCoach.UserId);
        }

        [Fact]
        public async Task AddCoachAsync_InvalidCoach_ThrowsException()
        {
            // Arrange
            var coach = new Data.Models.Coach { UserId = Guid.NewGuid() }; // Thiếu Bio

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => _repository.AddCoachAsync(coach, CancellationToken.None));
        }

        [Fact]
        public async Task GetCoachByIdAsync_ExistingId_ReturnsCoach()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var coach = new Data.Models.Coach { UserId = coachId, Bio = "Test Bio" };
            await _context.Coaches.AddAsync(coach);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachByIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(coachId, result.UserId);
        }

        [Fact]
        public async Task GetCoachByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachByIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCoachAsync_ExistingCoach_UpdatesCoach()
        {
            // Arrange
            var coach = new Data.Models.Coach
            {
                UserId = Guid.NewGuid(),
                Bio = "Updated Bio",
                RatePerHour = 60.00m
            };
            await _context.Coaches.AddAsync(coach);
            await _context.SaveChangesAsync();

            // Thay đổi thông tin coach
            coach.Bio = "New Updated Bio";
            coach.RatePerHour = 70.00m;

            // Act
            await _repository.UpdateCoachAsync(coach, CancellationToken.None);

            // Assert
            var updatedCoach = await _context.Coaches.FindAsync(coach.UserId);
            Assert.NotNull(updatedCoach);
            Assert.Equal("New Updated Bio", updatedCoach.Bio);
            Assert.Equal(70.00m, updatedCoach.RatePerHour);
        }

        [Fact]
        public async Task CoachExistsAsync_ExistingCoach_ReturnsTrue()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var coach = new Data.Models.Coach { UserId = coachId, Bio = "Test Bio" };
            await _context.Coaches.AddAsync(coach);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CoachExistsAsync(coachId, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CoachExistsAsync_NonExistingCoach_ReturnsFalse()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            // Act
            var result = await _repository.CoachExistsAsync(coachId, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllCoachesAsync_CoachesExist_ReturnsCoaches()
        {
            // Arrange
            var coaches = new List<Data.Models.Coach>
            {
                new Data.Models.Coach { UserId = Guid.NewGuid(), Bio = "Coach 1" },
                new Data.Models.Coach { UserId = Guid.NewGuid(), Bio = "Coach 2" }
            };
            await _context.Coaches.AddRangeAsync(coaches);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllCoachesAsync(CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllCoachesAsync_NoCoaches_ReturnsEmptyList()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                .UseInMemoryDatabase(databaseName: "CoachDatabase")
                .Options;

            // Create a new context using the in-memory database
            var context = new CoachDbContext(options);

            // Create repository
            var repository = new CoachRepository(context);

            // Act
            var result = await repository.GetAllCoachesAsync(CancellationToken.None);

            // Assert
            Assert.Empty(result); // Should return an empty list
        }
    }
}
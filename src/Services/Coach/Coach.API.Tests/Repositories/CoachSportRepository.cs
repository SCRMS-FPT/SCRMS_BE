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
    public class CoachSportRepositoryTests
    {
        private readonly CoachDbContext _context;
        private readonly CoachSportRepository _repository;

        public CoachSportRepositoryTests()
        {
            // Set up the InMemory database for testing
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Create a unique database name for each test run
                .Options;

            _context = new CoachDbContext(options);
            _repository = new CoachSportRepository(_context);
        }

        [Fact]
        public async Task AddCoachSportAsync_ValidCoachSport_AddsCoachSport()
        {
            // Arrange
            var coachSport = new CoachSport
            {
                CoachId = Guid.NewGuid(),
                SportId = Guid.NewGuid()
            };

            // Act
            await _repository.AddCoachSportAsync(coachSport, CancellationToken.None);
            var result = await _context.CoachSports.FindAsync(coachSport.CoachId, coachSport.SportId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(coachSport.CoachId, result.CoachId);
            Assert.Equal(coachSport.SportId, result.SportId);
        }

        [Fact]
        public async Task AddCoachSportAsync_InvalidCoachSport_ThrowsException()
        {
            // Arrange
            var coachSport = new CoachSport { CoachId = Guid.NewGuid() }; // Missing SportId

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // This action will invoke the validation logic where the exception should be triggered
                await _repository.AddCoachSportAsync(coachSport, CancellationToken.None);
            });

            // Verify the exception message contains the validation error for SportId
            Assert.Contains("SportId is required", exception.Message);
        }

        [Fact]
        public async Task GetCoachSportsByCoachIdAsync_CoachWithCoachSports_ReturnsCoachSports()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var coachSports = new List<CoachSport>
            {
                new CoachSport { CoachId = coachId, SportId = Guid.NewGuid() },
                new CoachSport { CoachId = coachId, SportId = Guid.NewGuid() }
            };
            await _context.CoachSports.AddRangeAsync(coachSports);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachSportsByCoachIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, cs => Assert.Equal(coachId, cs.CoachId));
        }

        [Fact]
        public async Task GetCoachSportsByCoachIdAsync_CoachWithoutCoachSports_ReturnsEmptyList()
        {
            // Arrange
            var coachId = Guid.NewGuid(); // No coach sports in the database

            // Act
            var result = await _repository.GetCoachSportsByCoachIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteCoachSportAsync_ExistingCoachSport_DeletesCoachSport()
        {
            // Arrange
            var coachSport = new CoachSport { CoachId = Guid.NewGuid(), SportId = Guid.NewGuid() };
            await _context.CoachSports.AddAsync(coachSport);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteCoachSportAsync(coachSport, CancellationToken.None);
            var result = await _context.CoachSports.FindAsync(coachSport.CoachId, coachSport.SportId);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Null(result); // The coach sport should be deleted
        }

        [Fact]
        public async Task DeleteCoachSportAsync_NonExistingCoachSport_ThrowsException()
        {
            // Arrange
            var coachSport = new CoachSport { CoachId = Guid.NewGuid(), SportId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.DeleteCoachSportAsync(coachSport, CancellationToken.None));
        }
    }
}
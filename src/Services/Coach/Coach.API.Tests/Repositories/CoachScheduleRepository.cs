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
    public class CoachScheduleRepositoryTests
    {
        private readonly CoachDbContext _context;
        private readonly CoachScheduleRepository _repository;

        public CoachScheduleRepositoryTests()
        {
            // Use InMemory database for testing
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                .UseInMemoryDatabase(databaseName: "CoachScheduleTestDb")
                .Options;

            _context = new CoachDbContext(options);
            _repository = new CoachScheduleRepository(_context);
        }

        [Fact]
        public async Task AddCoachScheduleAsync_ValidSchedule_AddsSchedule()
        {
            // Arrange
            var schedule = new CoachSchedule
            {
                Id = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                DayOfWeek = 1,
                StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)),
                EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10))
            };

            // Act
            await _repository.AddCoachScheduleAsync(schedule, CancellationToken.None);

            // Assert
            var addedSchedule = await _context.CoachSchedules.FindAsync(schedule.Id);
            Assert.NotNull(addedSchedule);
            Assert.Equal(schedule.CoachId, addedSchedule.CoachId);
        }

        [Fact]
        public async Task AddCoachScheduleAsync_InvalidSchedule_ThrowsException()
        {
            // Arrange
            var schedule = new CoachSchedule { Id = Guid.NewGuid() }; // Missing CoachId, DayOfWeek

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // This action will invoke SaveChangesAsync() where the validation logic should trigger
                await _repository.AddCoachScheduleAsync(schedule, CancellationToken.None);
            });

            // Verify the exception message
            Assert.Contains("CoachId is required", exception.Message);
            Assert.Contains("DayOfWeek is required", exception.Message);
        }

        [Fact]
        public async Task GetCoachScheduleByIdAsync_ExistingId_ReturnsSchedule()
        {
            // Arrange
            var scheduleId = Guid.NewGuid();
            var schedule = new CoachSchedule { Id = scheduleId, CoachId = Guid.NewGuid() };
            await _context.CoachSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachScheduleByIdAsync(scheduleId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(scheduleId, result.Id);
        }

        [Fact]
        public async Task GetCoachScheduleByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var scheduleId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachScheduleByIdAsync(scheduleId, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCoachScheduleAsync_ExistingSchedule_UpdatesSchedule()
        {
            // Arrange
            var schedule = new CoachSchedule
            {
                Id = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                DayOfWeek = 2,
                StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11))
            };

            await _context.CoachSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            schedule.DayOfWeek = 3;  // Update the schedule

            // Act
            await _repository.UpdateCoachScheduleAsync(schedule, CancellationToken.None);

            // Assert
            var updatedSchedule = await _context.CoachSchedules.FindAsync(schedule.Id);
            Assert.NotNull(updatedSchedule);
            Assert.Equal(3, updatedSchedule.DayOfWeek);
        }

        [Fact]
        public async Task DeleteCoachScheduleAsync_ExistingSchedule_DeletesSchedule()
        {
            // Arrange
            var schedule = new CoachSchedule { Id = Guid.NewGuid(), CoachId = Guid.NewGuid() };
            await _context.CoachSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteCoachScheduleAsync(schedule, CancellationToken.None);
            await _context.SaveChangesAsync(); // Ensure changes are saved to the in-memory database

            // Assert
            var deletedSchedule = await _context.CoachSchedules.FindAsync(schedule.Id);
            Assert.Null(deletedSchedule); // Ensure the schedule is truly deleted
        }

        [Fact]
        public async Task GetCoachSchedulesByCoachIdAsync_CoachWithSchedules_ReturnsSchedules()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var schedules = new List<CoachSchedule>
            {
                new CoachSchedule { Id = Guid.NewGuid(), CoachId = coachId },
                new CoachSchedule { Id = Guid.NewGuid(), CoachId = coachId }
            };
            await _context.CoachSchedules.AddRangeAsync(schedules);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachSchedulesByCoachIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, s => Assert.Equal(coachId, s.CoachId));
        }

        [Fact]
        public async Task HasCoachScheduleConflictAsync_ConflictingSchedule_ReturnsTrue()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var dayOfWeek = 1;
            var startTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10));
            var endTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11));

            var existingSchedule = new CoachSchedule
            {
                CoachId = coachId,
                DayOfWeek = dayOfWeek,
                StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11))
            };

            await _context.CoachSchedules.AddAsync(existingSchedule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasCoachScheduleConflictAsync(coachId, dayOfWeek, startTime, endTime, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasCoachScheduleConflictAsync_NoConflict_ReturnsFalse()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var dayOfWeek = 1;
            var startTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(12));
            var endTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(13));

            var existingSchedule = new CoachSchedule
            {
                CoachId = coachId,
                DayOfWeek = dayOfWeek,
                StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11))
            };

            await _context.CoachSchedules.AddAsync(existingSchedule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasCoachScheduleConflictAsync(coachId, dayOfWeek, startTime, endTime, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
    }
}
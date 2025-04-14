using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using CourtBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Infrastructure.Data.Repositories
{
    public class CourtScheduleRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly CourtScheduleRepository _repository;

        public CourtScheduleRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CourtScheduleInMemoryDb_" + Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(_options);
            _repository = new CourtScheduleRepository(_context);
        }

        [Fact]
        public async Task AddCourtScheduleAsync_ShouldAddScheduleToDatabase()
        {
            // Arrange
            var court = await CreateTestCourtAsync();
            var schedule = CreateTestSchedule(court.Id);

            // Act
            await _repository.AddCourtScheduleAsync(schedule, CancellationToken.None);

            // Assert
            var savedEntity = await _context.CourtSchedules.FindAsync(schedule.Id);
            Assert.NotNull(savedEntity);
            Assert.Equal(schedule.Id, savedEntity.Id);
        }

        [Fact]
        public async Task GetCourtScheduleByIdAsync_ShouldReturnCorrectSchedule()
        {
            // Arrange
            var court = await CreateTestCourtAsync();
            var schedule = CreateTestSchedule(court.Id);
            await _context.CourtSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCourtScheduleByIdAsync(schedule.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(schedule.Id, result.Id);
            Assert.Equal(schedule.CourtId, result.CourtId);
        }

        [Fact]
        public async Task UpdateCourtScheduleAsync_ShouldUpdateScheduleInDatabase()
        {
            // Arrange
            var court = await CreateTestCourtAsync();
            var schedule = CreateTestSchedule(court.Id);
            await _context.CourtSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            var newPrice = 200.0m;
            schedule.Update(schedule.DayOfWeek, schedule.StartTime, schedule.EndTime, newPrice, schedule.Status);

            // Act
            await _repository.UpdateCourtScheduleAsync(schedule, CancellationToken.None);

            // Assert
            var updatedEntity = await _context.CourtSchedules.FindAsync(schedule.Id);
            Assert.Equal(newPrice, updatedEntity.PriceSlot);
        }

        [Fact]
        public async Task DeleteCourtScheduleAsync_ShouldRemoveScheduleFromDatabase()
        {
            // Arrange
            var court = await CreateTestCourtAsync();
            var schedule = CreateTestSchedule(court.Id);
            await _context.CourtSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteCourtScheduleAsync(schedule.Id, CancellationToken.None);

            // Assert
            var deletedEntity = await _context.CourtSchedules.FindAsync(schedule.Id);
            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task GetSchedulesByCourtIdAsync_ShouldReturnAllSchedulesForCourt()
        {
            // Arrange
            var court1 = await CreateTestCourtAsync();
            var court2 = await CreateTestCourtAsync();

            var schedule1 = CreateTestSchedule(court1.Id);
            var schedule2 = CreateTestSchedule(court1.Id);
            var schedule3 = CreateTestSchedule(court2.Id);

            await _context.CourtSchedules.AddRangeAsync(schedule1, schedule2, schedule3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSchedulesByCourtIdAsync(court1.Id, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Equal(court1.Id, s.CourtId));
        }

        private async Task<Court> CreateTestCourtAsync()
        {
            var sportCenterId = SportCenterId.Of(Guid.NewGuid());
            var sportId = SportId.Of(Guid.NewGuid());
            var courtId = CourtId.Of(Guid.NewGuid());

            var court = Court.Create(
                courtId,
                CourtName.Of("Test Court"),
                sportCenterId,
                sportId,
                TimeSpan.FromMinutes(60),
                "Test Description",
                "[]",
                CourtType.Indoor,
                30
            );

            await _context.Courts.AddAsync(court);
            await _context.SaveChangesAsync();

            return court;
        }

        private CourtSchedule CreateTestSchedule(CourtId courtId)
        {
            return CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                courtId,
                DayOfWeekValue.Of(new List<int> { 1, 2, 3 }),
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(12),
                100.0m
            );
        }
    }
}
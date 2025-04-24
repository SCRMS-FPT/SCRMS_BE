using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using CourtBooking.Infrastructure.Data;
using CourtBooking.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Infrastructure.Data.Repositories
{
    [Collection("PostgresDatabase")]
    public class CourtScheduleRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ICourtScheduleRepository _repository;
        private readonly PostgresTestFixture _fixture;

        public CourtScheduleRepositoryTests(PostgresTestFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.CreateContext();
            _repository = new CourtScheduleRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task AddCourtScheduleAsync_ShouldAddScheduleToDatabase()
        {
            // Arrange
            var court = await CreateTestCourt();
            var schedule = CreateTestSchedule(court.Id);

            // Act
            await _repository.AddCourtScheduleAsync(schedule, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var savedSchedule = await _context.CourtSchedules.AsNoTracking().FirstOrDefaultAsync();
            Assert.NotNull(savedSchedule);
            Assert.Equal(schedule.Id.Value, savedSchedule.Id.Value);
        }

        [Fact]
        public async Task GetCourtScheduleByIdAsync_ShouldReturnCorrectSchedule()
        {
            // Arrange
            var court = await CreateTestCourt();
            var schedule = CreateTestSchedule(court.Id);
            _context.CourtSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Clear the change tracker
            _context.ChangeTracker.Clear();

            // Act
            var result = await _repository.GetCourtScheduleByIdAsync(schedule.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(schedule.Id.Value, result.Id.Value);
            Assert.Equal(schedule.CourtId.Value, result.CourtId.Value);
        }

        [Fact]
        public async Task UpdateCourtScheduleAsync_ShouldUpdateScheduleInDatabase()
        {
            // Arrange
            var court = await CreateTestCourt();
            var schedule = CreateTestSchedule(court.Id);
            _context.CourtSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Clear the change tracker
            _context.ChangeTracker.Clear();

            // Create a new schedule with same ID but different end time
            var newEndTime = TimeSpan.FromHours(18);
            var updatedSchedule = CourtSchedule.Create(
                schedule.Id,
                schedule.CourtId,
                schedule.DayOfWeek,
                schedule.StartTime,
                newEndTime,
                schedule.PriceSlot
            );

            // Act
            await _repository.UpdateCourtScheduleAsync(updatedSchedule, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.CourtSchedules.AsNoTracking().FirstOrDefaultAsync(s => s.Id == schedule.Id);
            Assert.NotNull(result);
            Assert.Equal(newEndTime, result.EndTime);
        }

        [Fact]
        public async Task DeleteCourtScheduleAsync_ShouldRemoveScheduleFromDatabase()
        {
            // Arrange
            var court = await CreateTestCourt();
            var schedule = CreateTestSchedule(court.Id);
            _context.CourtSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Clear the change tracker
            _context.ChangeTracker.Clear();

            // Act
            await _repository.DeleteCourtScheduleAsync(schedule.Id, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.CourtSchedules.AsNoTracking().FirstOrDefaultAsync(s => s.Id == schedule.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSchedulesByCourtIdAsync_ShouldReturnAllSchedulesForCourt()
        {
            // Arrange
            var court1 = await CreateTestCourt();
            var court2 = await CreateTestCourt();

            var schedule1 = CreateTestSchedule(court1.Id);
            var schedule2 = CreateTestSchedule(court1.Id);
            var schedule3 = CreateTestSchedule(court2.Id);

            _context.CourtSchedules.AddRange(new[] { schedule1, schedule2, schedule3 });
            await _context.SaveChangesAsync();

            // Clear the change tracker
            _context.ChangeTracker.Clear();

            // Act
            var result = await _repository.GetSchedulesByCourtIdAsync(court1.Id, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Equal(court1.Id.Value, s.CourtId.Value));
        }

        private async Task<Court> CreateTestCourt()
        {
            var sport = await CreateTestSport();
            var sportCenter = await CreateTestSportCenter();

            var court = Court.Create(
                CourtId.Of(Guid.NewGuid()),
                CourtName.Of("Test Court"),
                sportCenter.Id,
                sport.Id,
                TimeSpan.FromMinutes(60),
                "Test Court Description",
                "[]",
                CourtType.Indoor,
                50
            );

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            // Clear the change tracker
            _context.ChangeTracker.Clear();

            return court;
        }

        private async Task<Sport> CreateTestSport()
        {
            var sport = Sport.Create(
                SportId.Of(Guid.NewGuid()),
                "Test Sport",
                "Test Sport Description",
                "icon.png"
            );

            _context.Sports.Add(sport);
            await _context.SaveChangesAsync();

            // Clear the change tracker
            _context.ChangeTracker.Clear();

            return sport;
        }

        private async Task<SportCenter> CreateTestSportCenter()
        {
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(Guid.NewGuid()),
                OwnerId.Of(Guid.NewGuid()),
                "Test Sport Center",
                "0123456789",
                new Location("123 Main St", "HCMC", "Vietnam", "70000"),
                new GeoLocation(10.762622, 106.660172),
                new SportCenterImages("main.jpg", new List<string> { "1.jpg", "2.jpg" }),
                "A test sport center"
            );

            _context.SportCenters.Add(sportCenter);
            await _context.SaveChangesAsync();

            // Clear the change tracker
            _context.ChangeTracker.Clear();

            return sportCenter;
        }

        private CourtSchedule CreateTestSchedule(CourtId courtId)
        {
            return CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                courtId,
                DayOfWeekValue.Of(new List<int> { 1, 2, 3 }), // Monday, Tuesday, Wednesday
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(12),
                100.0m
            );
        }

        private void CleanDays()
        {
            var days = DayOfWeekValue.Of(new List<int> { 1, 2, 3 });
            Assert.Equal(3, days.Days.Count);
            Assert.All(days.Days, day => Assert.InRange(day, 1, 7));
        }
    }
}
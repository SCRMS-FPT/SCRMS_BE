using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CourtBooking.Infrastructure.Data;
using CourtBooking.Test.Infrastructure.Data;

namespace CourtBooking.Test.Application.Repositories
{
    [Collection("PostgresDatabase")]
    public class CourtScheduleRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CourtScheduleRepository _repository;
        private readonly PostgresTestFixture _fixture;
        private readonly IDbContextTransaction _transaction;

        public CourtScheduleRepositoryTests(PostgresTestFixture fixture)
        {
            _fixture = fixture;
            _context = new ApplicationDbContext(_fixture.ContextOptions);
            _transaction = _context.Database.BeginTransaction();
            _repository = new CourtScheduleRepository(_context);
            CleanTestData();
        }

        public void Dispose()
        {
            _transaction.Rollback();
            _context.Dispose();
        }

        [Fact]
        public async Task AddCourtScheduleAsync_Should_PersistData()
        {
            // Arrange
            var court = await CreateTestCourt();
            var schedule = CreateTestSchedule(court.Id);

            // Act
            await _repository.AddCourtScheduleAsync(schedule, CancellationToken.None);

            // Assert
            var savedSchedule = await _context.CourtSchedules.FirstAsync();
            Assert.NotNull(savedSchedule);
            Assert.Equal(schedule.Id, savedSchedule.Id);
            Assert.Equal(schedule.CourtId, savedSchedule.CourtId);
            Assert.Equal(schedule.StartTime, savedSchedule.StartTime);
            Assert.Equal(schedule.EndTime, savedSchedule.EndTime);
            Assert.Equal(schedule.PriceSlot, savedSchedule.PriceSlot);
            Assert.Equal(schedule.Status, savedSchedule.Status);
        }

        [Fact]
        public async Task GetCourtScheduleByIdAsync_Should_ReturnCorrectEntity()
        {
            // Arrange
            var court = await CreateTestCourt();
            var schedule = CreateTestSchedule(court.Id);
            await _repository.AddCourtScheduleAsync(schedule, CancellationToken.None);

            // Act
            var result = await _repository.GetCourtScheduleByIdAsync(schedule.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(schedule.Id, result.Id);
            Assert.Equal(schedule.CourtId, result.CourtId);
            Assert.Equal(schedule.StartTime, result.StartTime);
            Assert.Equal(schedule.EndTime, result.EndTime);
            Assert.Equal(schedule.PriceSlot, result.PriceSlot);
            Assert.Equal(schedule.Status, result.Status);
        }

        [Fact]
        public async Task UpdateCourtScheduleAsync_Should_ModifyExistingEntity()
        {
            // Arrange
            var court = await CreateTestCourt();
            var schedule = CreateTestSchedule(court.Id);
            await _repository.AddCourtScheduleAsync(schedule, CancellationToken.None);

            // Act
            var newPriceSlot = 200m;
            var newStatus = CourtScheduleStatus.Maintenance;
            schedule.Update(schedule.DayOfWeek, schedule.StartTime, schedule.EndTime, newPriceSlot, newStatus);
            await _repository.UpdateCourtScheduleAsync(schedule, CancellationToken.None);

            // Assert
            var updatedSchedule = await _context.CourtSchedules.FirstAsync();
            Assert.NotNull(updatedSchedule);
            Assert.Equal(newPriceSlot, updatedSchedule.PriceSlot);
            Assert.Equal(newStatus, updatedSchedule.Status);
        }

        [Fact]
        public async Task DeleteCourtScheduleAsync_Should_RemoveEntity()
        {
            // Arrange
            var court = await CreateTestCourt();
            var schedule = CreateTestSchedule(court.Id);
            await _repository.AddCourtScheduleAsync(schedule, CancellationToken.None);

            // Act
            await _repository.DeleteCourtScheduleAsync(schedule.Id, CancellationToken.None);

            // Assert
            var deletedSchedule = await _context.CourtSchedules.FirstOrDefaultAsync();
            Assert.Null(deletedSchedule);
        }

        [Fact]
        public async Task GetCourtSchedulesByCourtIdAsync_Should_FilterCorrectly()
        {
            // Arrange
            var court1 = await CreateTestCourt();
            var court2 = await CreateTestCourt();
            var schedule1 = CreateTestSchedule(court1.Id);
            var schedule2 = CreateTestSchedule(court1.Id);
            var schedule3 = CreateTestSchedule(court2.Id);
            await _repository.AddCourtScheduleAsync(schedule1, CancellationToken.None);
            await _repository.AddCourtScheduleAsync(schedule2, CancellationToken.None);
            await _repository.AddCourtScheduleAsync(schedule3, CancellationToken.None);

            // Act
            var result = await _repository.GetSchedulesByCourtIdAsync(court1.Id, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Equal(court1.Id, s.CourtId));
        }

        [Fact]
        public async Task GetCourtSchedulesByCourtIdAsync_Should_SupportPagination()
        {
            // Arrange
            var court = await CreateTestCourt();
            for (int i = 0; i < 5; i++)
            {
                var schedule = CreateTestSchedule(court.Id);
                await _repository.AddCourtScheduleAsync(schedule, CancellationToken.None);
            }

            // Act
            var result = await _repository.GetSchedulesByCourtIdAsync(court.Id, CancellationToken.None);

            // Assert
            Assert.Equal(5, result.Count);
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

            return sport;
        }

        private async Task<SportCenter> CreateTestSportCenter()
        {
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(Guid.NewGuid()),
                OwnerId.Of(Guid.NewGuid()),
                "Tennis Center",
                "0123456789",
                new Location("123 Main St", "HCMC", "Vietnam", "70000"),
                new GeoLocation(10.762622, 106.660172),
                new SportCenterImages("main.jpg", new List<string> { "1.jpg", "2.jpg" }),
                "A great tennis center"
            );

            _context.SportCenters.Add(sportCenter);
            await _context.SaveChangesAsync();

            return sportCenter;
        }

        private async Task<Court> CreateTestCourt()
        {
            var sport = await CreateTestSport();
            var sportcenter = await CreateTestSportCenter();

            var court = Court.Create(
                CourtId.Of(Guid.NewGuid()),
                new CourtName("Test Court"),
                sportcenter.Id,
                sport.Id,
                TimeSpan.FromMinutes(60),
                "Description",
                "[]", // JSON array rỗng cho facilities
                CourtType.Outdoor,
                50
            );

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            return court;
        }

        private CourtSchedule CreateTestSchedule(CourtId courtId)
        {
            return CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                courtId,
                DayOfWeekValue.Of(new List<int> { 1, 2, 3, 4, 5 }),
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(12),
                100.0m
            );
        }

        private async Task<CourtSchedule> CreateAndSaveSchedule(CourtId courtId)
        {
            var schedule = CreateTestSchedule(courtId);
            _context.CourtSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        private void CleanTestData()
        {
            _context.CourtSchedules.RemoveRange(_context.CourtSchedules);
            _context.Courts.RemoveRange(_context.Courts);
        }

        private void CleanDays()
        {
            var days = DayOfWeekValue.Of(new List<int> { 1, 2, 3 });
            Assert.Equal(3, days.Days.Count);
        }
    }
}
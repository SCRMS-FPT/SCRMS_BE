using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using CourtBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CourtBooking.Application.Data;

namespace CourtBooking.Test.Application.Repositories
{
    public class CourtScheduleRepositoryTests : IDisposable
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Database=courtbooking_test;Username=postgres;Password=123456";
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly CourtScheduleRepository _repository;
        private readonly IDbContextTransaction _transaction;

        public CourtScheduleRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            _context = new ApplicationDbContext(_options);
            _context.Database.EnsureCreated();
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
            await _context.SaveChangesAsync();

            // Assert
            var savedSchedule = _context.CourtSchedules.First();
            Assert.Equal(schedule.StartTime, savedSchedule.StartTime);
            Assert.Equal(court.Id, savedSchedule.CourtId);
        }

        [Fact]
        public async Task GetCourtScheduleByIdAsync_Should_ReturnCorrectEntity()
        {
            // Arrange
            var court = await CreateTestCourt();
            var expected = await CreateAndSaveSchedule(court.Id);

            // Act
            var result = await _repository.GetCourtScheduleByIdAsync(expected.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.PriceSlot, result.PriceSlot);
            Assert.Equal(expected.DayOfWeek.Days, result.DayOfWeek.Days);
        }

        [Fact]
        public async Task UpdateCourtScheduleAsync_Should_ModifyExistingRecord()
        {
            // Arrange
            var court = await CreateTestCourt();
            var original = await CreateAndSaveSchedule(court.Id);
            var updatedPrice = 150m;

            // Act
            original.Update(original.DayOfWeek, original.StartTime, original.EndTime, updatedPrice, original.Status);
            await _repository.UpdateCourtScheduleAsync(original, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var updatedSchedule = await _context.CourtSchedules.FindAsync(original.Id);
            Assert.Equal(updatedPrice, updatedSchedule.PriceSlot);
        }

        [Fact]
        public async Task DeleteCourtScheduleAsync_Should_RemoveRecord()
        {
            // Arrange
            var court = await CreateTestCourt();
            var schedule = await CreateAndSaveSchedule(court.Id);

            // Act
            await _repository.DeleteCourtScheduleAsync(schedule.Id, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Empty(_context.CourtSchedules);
        }

        [Fact]
        public async Task GetSchedulesByCourtIdAsync_Should_FilterCorrectly()
        {
            // Arrange
            var court1 = await CreateTestCourt();
            var court2 = await CreateTestCourt();
            var testData = new List<CourtSchedule> {
                await CreateAndSaveSchedule(court1.Id),
                await CreateAndSaveSchedule(court1.Id),
                await CreateAndSaveSchedule(court2.Id)
            };

            // Act
            var results = await _repository.GetSchedulesByCourtIdAsync(court1.Id, CancellationToken.None);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, s => Assert.Equal(court1.Id, s.CourtId));
        }

        [Fact]
        public async Task GetSchedulesByCourt_Should_SupportPagination()
        {
            // Arrange
            var court = await CreateTestCourt();
            var testData = Enumerable.Range(1, 15)
                .Select(async _ => await CreateAndSaveSchedule(court.Id))
                .ToList();

            await Task.WhenAll(testData);

            // Act
            var results = await _repository.GetSchedulesByCourt(court.Id, CancellationToken.None);

            // Assert
            Assert.Equal(15, results.Count()); // Use Count() as a method call, not as a property
        }

        private async Task<Court> CreateTestCourt()
        {
            var sport = Sport.Create(
                SportId.Of(Guid.NewGuid()),
                "Tennis",
                "Tennis description",
                "tennis.png"
            );

            var id = SportCenterId.Of(Guid.NewGuid());
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var name = "Tennis Center";
            var phoneNumber = "0123456789";
            var address = new Location("123 Main St", "HCMC", "Vietnam", "70000");
            var location = new GeoLocation(10.762622, 106.660172);
            var images = new SportCenterImages("main.jpg", new List<string> { "1.jpg", "2.jpg" });
            var description = "A great tennis center";

            // Act
            var sportCenter = SportCenter.Create(id, ownerId, name, phoneNumber, address, location, images, description);

            var court = Court.Create(
                CourtId.Of(Guid.NewGuid()),
                new CourtName("Court 1"),
                sportCenter.Id,
                sport.Id,
                TimeSpan.FromHours(2),
                "Court description",
                "[]",
                CourtType.Indoor,
                100
            );

            _context.Sports.Add(sport);
            _context.SportCenters.Add(sportCenter);
            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            return court;
        }

        private CourtSchedule CreateTestSchedule(CourtId courtId, decimal price = 100m)
        {
            return CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                courtId,
                DayOfWeekValue.Of(new List<int> { 1, 3, 5 }),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(12),
                price
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
            // Use proper int values, not method references
            var days = DayOfWeekValue.Of(new List<int> { 1, 2, 3 });
            Assert.Equal(3, days.Days.Count);
        }
    }
}
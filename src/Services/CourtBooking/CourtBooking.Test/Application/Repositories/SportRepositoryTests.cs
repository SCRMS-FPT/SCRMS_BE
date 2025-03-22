using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CourtBooking.Infrastructure.Data;

namespace CourtBooking.Test.Application.Repositories
{
    public class SportRepositoryTests : IDisposable
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Database=courtbooking_test;Username=postgres;Password=123456";
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly SportRepository _repository;

        public SportRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            _context = new ApplicationDbContext(_options);
            _context.Database.EnsureCreated();

            _repository = new SportRepository(_context);

            // Cleanup trước khi chạy test
            _context.Database.BeginTransaction();
            CleanupTestData();
        }

        public void Dispose()
        {
            _context.Database.RollbackTransaction();
            _context.Dispose();
        }

        [Fact]
        public async Task AddSportAsync_Should_PersistData()
        {
            // Arrange
            var sport = CreateValidSport();

            // Act
            await _repository.AddSportAsync(sport, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var savedSport = _context.Sports.First();
            Assert.Equal(sport.Name, savedSport.Name);
            Assert.Equal(sport.Description, savedSport.Description);
        }

        [Fact]
        public async Task GetSportByIdAsync_Should_ReturnCorrectEntity()
        {
            // Arrange
            var expected = await CreateAndSaveSport();

            // Act
            var result = await _repository.GetSportByIdAsync(expected.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Id.Value, result.Id.Value);
            Assert.Equal(expected.Name, result.Name);
        }

        [Fact]
        public async Task UpdateSportAsync_Should_ModifyExistingRecord()
        {
            // Arrange
            var original = await CreateAndSaveSport();
            var updatedName = "Updated Sport Name";

            // Act
            original.Update(updatedName, original.Description, original.Icon);
            await _repository.UpdateSportAsync(original, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var updatedSport = await _context.Sports.FindAsync(original.Id);
            Assert.Equal(updatedName, updatedSport.Name);
        }

        [Fact]
        public async Task DeleteSportAsync_Should_RemoveSport()
        {
            // Arrange
            var sport = await CreateAndSaveSport();

            // Act
            await _repository.DeleteSportAsync(sport.Id, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Empty(_context.Sports);
        }

        [Fact]
        public async Task GetAllSportsAsync_Should_ReturnAllRecords()
        {
            // Arrange
            var testData = await CreateMultipleSports(5);

            // Act
            var result = await _repository.GetAllSportsAsync(CancellationToken.None);

            // Assert
            Assert.Equal(testData.Count, result.Count);
            Assert.All(testData, item =>
                Assert.Contains(result, r => r.Id == item.Id));
        }

        [Fact]
        public async Task IsSportInUseAsync_Should_DetectUsedSports()
        {
            // Arrange
            var sport = await CreateAndSaveSport();
            await CreateCourtForSport(sport.Id);

            // Act
            var result = await _repository.IsSportInUseAsync(sport.Id, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetSportsByIdsAsync_Should_FilterCorrectly()
        {
            // Arrange
            var sports = await CreateMultipleSports(5);
            var targetIds = new List<SportId> { sports[1].Id, sports[3].Id };

            // Act
            var result = await _repository.GetSportsByIdsAsync(targetIds, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Id == targetIds[0]);
            Assert.Contains(result, r => r.Id == targetIds[1]);
        }

        private async Task<Sport> CreateAndSaveSport()
        {
            var sport = CreateValidSport();
            _context.Sports.Add(sport);
            await _context.SaveChangesAsync();
            return sport;
        }

        private async Task<List<Sport>> CreateMultipleSports(int count)
        {
            var sports = Enumerable.Range(1, count)
                .Select(_ => CreateValidSport())
                .ToList();

            _context.Sports.AddRange(sports);
            await _context.SaveChangesAsync();
            return sports;
        }

        private async Task<Court> CreateCourtForSport(SportId sportId)
        {
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
                new CourtName("Test Court"),
                sportCenter.Id,
                sportId,
                TimeSpan.FromMinutes(60),
                "Test description",
                "[]",
                CourtType.Indoor,
                50
            );

            _context.SportCenters.Add(sportCenter);
            _context.Courts.Add(court);
            await _context.SaveChangesAsync();
            return court;
        }

        private void CleanupTestData()
        {
            _context.Courts.RemoveRange(_context.Courts);
            _context.SportCenters.RemoveRange(_context.SportCenters);
            _context.Sports.RemoveRange(_context.Sports);
            _context.SaveChanges();
        }

        private Sport CreateValidSport()
        {
            return Sport.Create(
                SportId.Of(Guid.NewGuid()),
                "Tennis",
                "Racket sport played individually or in teams",
                "tennis-icon.png"
            );
        }
    }
}
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
using CourtBooking.Test.Infrastructure.Data;

namespace CourtBooking.Test.Application.Repositories
{
    [Collection("PostgresDatabase")]
    public class SportRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly SportRepository _repository;
        private readonly PostgresTestFixture _fixture;

        public SportRepositoryTests(PostgresTestFixture fixture)
        {
            _fixture = fixture;
            _context = new ApplicationDbContext(_fixture.ContextOptions);
            _repository = new SportRepository(_context);
        }

        public void Dispose()
        {
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
            var result = await _context.Sports.FindAsync(sport.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllSportsAsync_Should_ReturnAllRecords()
        {
            // Arrange
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE sports CASCADE");
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

        private Sport CreateValidSport()
        {
            return Sport.Create(
                SportId.Of(Guid.NewGuid()),
                "Test Sport",
                "Test Sport Description",
                "icon.png"
            );
        }

        private async Task<SportCenter> CreateAndSaveSportCenter()
        {
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(Guid.NewGuid()),
                OwnerId.Of(Guid.NewGuid()),
                "Test Sport Center",
                "0123456789",
                new Location("123 Main St", "HCMC", "Vietnam", "70000"),
                new GeoLocation(10.762622, 106.660172),
                new SportCenterImages("main.jpg", new List<string> { "1.jpg", "2.jpg" }),
                "Test Sport Center Description"
            );
            _context.SportCenters.Add(sportCenter);
            await _context.SaveChangesAsync();
            return sportCenter;
        }

        private async Task CreateCourtForSport(SportId sportId)
        {
            var sportCenter = await CreateAndSaveSportCenter();
            var court = Court.Create(
                CourtId.Of(Guid.NewGuid()),
                CourtName.Of("Test Court"),
                sportCenter.Id,
                sportId,
                TimeSpan.FromHours(1),
                "Test Court Description",
                "[]",
                CourtType.Indoor,
                50,
                24,
                100
            );

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();
        }
    }
}
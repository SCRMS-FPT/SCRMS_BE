using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CourtBooking.Infrastructure.Data;
using CourtBooking.Test.Infrastructure.Data;
using CourtBooking.Application.DTOs;
using FluentAssertions;

namespace CourtBooking.Test.Application.Repositories
{
    [Collection("PostgresDatabase")]
    public class SportCenterRepositoryTests : IDisposable
    {
        private readonly PostgresTestFixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly SportCenterRepository _repository;

        public SportCenterRepositoryTests(PostgresTestFixture fixture)
        {
            _fixture = fixture;
            _context = new ApplicationDbContext(_fixture.ContextOptions);
            _repository = new SportCenterRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task AddSportCenterAsync_Should_PersistData()
        {
            // Arrange
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenter = CreateValidSportCenter(ownerId);

            // Act
            await _repository.AddSportCenterAsync(sportCenter, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var savedSportCenter = await _context.SportCenters
                .FirstAsync(sc => sc.Id == sportCenter.Id);
            Assert.NotNull(savedSportCenter);
            Assert.Equal(sportCenter.Name, savedSportCenter.Name);
            Assert.Equal(sportCenter.PhoneNumber, savedSportCenter.PhoneNumber);
        }

        [Fact]
        public async Task GetSportCenterByIdAsync_Should_ReturnCorrectEntity()
        {
            // Arrange
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenter = await CreateAndSaveSportCenter(ownerId);

            // Act
            var result = await _repository.GetSportCenterByIdAsync(sportCenter.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sportCenter.Id, result.Id);
            Assert.Equal(sportCenter.Name, result.Name);
        }

        [Fact]
        public async Task GetSportCentersByOwnerIdAsync_Should_ReturnCorrectEntities()
        {
            // Arrange
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenter1 = await CreateAndSaveSportCenter(ownerId);
            var sportCenter2 = await CreateAndSaveSportCenter(ownerId);
            var otherOwnerId = OwnerId.Of(Guid.NewGuid());
            var otherSportCenter = await CreateAndSaveSportCenter(otherOwnerId);

            // Act
            var results = await _repository.GetSportCentersByOwnerIdAsync(ownerId.Value, CancellationToken.None);

            // Assert
            Assert.Equal(2, results.Count());
            Assert.All(results, sc => Assert.Equal(ownerId, sc.OwnerId));
            Assert.DoesNotContain(results, sc => sc.OwnerId == otherOwnerId);
        }

        [Fact]
        public async Task UpdateSportCenterAsync_Should_ModifyExistingEntity()
        {
            // Arrange
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenter = await CreateAndSaveSportCenter(ownerId);
            var newName = "Updated Sport Center";
            var newPhone = "9876543210";
            var newDescription = "Updated Description";

            // Act
            sportCenter.UpdateInfo(newName, newPhone, newDescription);
            await _repository.UpdateSportCenterAsync(sportCenter, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var updatedSportCenter = await _context.SportCenters
                .FirstAsync(sc => sc.Id == sportCenter.Id);
            Assert.Equal(newName, updatedSportCenter.Name);
            Assert.Equal(newPhone, updatedSportCenter.PhoneNumber);
            Assert.Equal(newDescription, updatedSportCenter.Description);
        }

        [Fact]
        public async Task DeleteSportCenterAsync_Should_RemoveSportCenter()
        {
            // Arrange
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenter = await CreateAndSaveSportCenter(ownerId);

            // Act
            await _repository.DeleteSportCenterAsync(sportCenter.Id, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.SportCenters.FindAsync(sportCenter.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPaginatedSportCentersAsync_Should_ReturnPaginatedRecords()
        {
            // Arrange
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenters = new List<SportCenter>();
            
            for (int i = 0; i < 10; i++)
            {
                var sportCenter = SportCenter.Create(
                    SportCenterId.Of(Guid.NewGuid()),
                    ownerId,
                    $"Sport Center {i}",
                    "123456789",
                    new Location("Street", "City", "District", "Country"),
                    new GeoLocation(0, 0),
                    new SportCenterImages("main.jpg", new List<string>()),
                    "Description"
                );
                sportCenters.Add(sportCenter);
            }
            await _context.SportCenters.AddRangeAsync(sportCenters);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPaginatedSportCentersAsync(0, 5, CancellationToken.None);

            // Assert
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task Create_Should_AcceptEmptyDescription_AndEmptyImages()
        {
            // Arrange
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(Guid.NewGuid()),
                ownerId,
                "Test Center",
                "0123456789",
                Location.Of("123 Test St", "Test City", "Test District", "Test Ward"),
                new GeoLocation(10.0, 20.0),
                new SportCenterImages("default.jpg", new List<string>()),
                string.Empty
            );

            // Act
            await _repository.AddSportCenterAsync(sportCenter, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var savedSportCenter = await _context.SportCenters
                .FirstOrDefaultAsync(s => s.Id == sportCenter.Id);
            Assert.NotNull(savedSportCenter);
            Assert.Equal(string.Empty, savedSportCenter.Description);
            Assert.Empty(savedSportCenter.Images.ImageUrls);
        }

        private async Task<SportCenter> CreateAndSaveSportCenter(OwnerId ownerId)
        {
            var sportCenter = CreateValidSportCenter(ownerId);
            await _context.SportCenters.AddAsync(sportCenter);
            await _context.SaveChangesAsync();
            return sportCenter;
        }

        private SportCenter CreateValidSportCenter(OwnerId ownerId)
        {
            return SportCenter.Create(
                SportCenterId.Of(Guid.NewGuid()),
                ownerId,
                "Test Sport Center",
                "0123456789",
                new Location("123 Main St", "HCMC", "Vietnam", "70000"),
                new GeoLocation(10.762622, 106.660172),
                new SportCenterImages("main.jpg", new List<string> { "1.jpg", "2.jpg" }),
                "Test Description"
            );
        }
    }
}


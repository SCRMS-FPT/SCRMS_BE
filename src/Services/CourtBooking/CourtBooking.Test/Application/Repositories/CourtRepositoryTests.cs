using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Infrastructure.Data;
using CourtBooking.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Repositories
{
    public class CourtRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ICourtRepository _repository;

        public CourtRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"CourtDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new CourtRepository(_context);
        }

        [Fact]
        public async Task GetCourtByIdAsync_Should_ReturnCourt_When_CourtExists()
        {
            // Arrange
            var courtId = CourtId.Of(Guid.NewGuid());
            var sportCenterId = SportCenterId.Of(Guid.NewGuid());
            var sportId = SportId.Of(Guid.NewGuid());

            var court = Court.Create(
                courtId,
                sportCenterId,
                sportId,
                CourtName.Create("Tennis Court 1"),
                "Main Court",
                100.0m,
                "Indoor",
                24,
                50
            );

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCourtByIdAsync(courtId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(courtId, result.Id);
            Assert.Equal("Tennis Court 1", result.CourtName.Value);
        }

        [Fact]
        public async Task GetCourtByIdAsync_Should_ReturnNull_When_CourtDoesNotExist()
        {
            // Arrange
            var nonExistingId = CourtId.Of(Guid.NewGuid());

            // Act
            var result = await _repository.GetCourtByIdAsync(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task IsOwnedByUserAsync_Should_ReturnTrue_When_UserOwnsSportCenter()
        {
            // Arrange
            var courtId = CourtId.Of(Guid.NewGuid());
            var sportCenterId = SportCenterId.Of(Guid.NewGuid());
            var sportId = SportId.Of(Guid.NewGuid());
            var ownerId = Guid.NewGuid();

            var sportCenter = SportCenter.Create(
                sportCenterId,
                "Sport Center 1",
                "123456789",
                "Description",
                new Location("Address", "City", "Province", "Country", "10000"),
                new GeoLocation(10.0, 20.0),
                ownerId,
                new SportCenterImages()
            );

            var court = Court.Create(
                courtId,
                sportCenterId,
                sportId,
                CourtName.Create("Tennis Court 1"),
                "Main Court",
                100.0m,
                "Indoor",
                24,
                50
            );

            _context.SportCenters.Add(sportCenter);
            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsOwnedByUserAsync(courtId.Value, ownerId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsOwnedByUserAsync_Should_ReturnFalse_When_UserDoesNotOwnSportCenter()
        {
            // Arrange
            var courtId = CourtId.Of(Guid.NewGuid());
            var sportCenterId = SportCenterId.Of(Guid.NewGuid());
            var sportId = SportId.Of(Guid.NewGuid());
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var sportCenter = SportCenter.Create(
                sportCenterId,
                "Sport Center 1",
                "123456789",
                "Description",
                new Location("Address", "City", "Province", "Country", "10000"),
                new GeoLocation(10.0, 20.0),
                ownerId,
                new SportCenterImages()
            );

            var court = Court.Create(
                courtId,
                sportCenterId,
                sportId,
                CourtName.Create("Tennis Court 1"),
                "Main Court",
                100.0m,
                "Indoor",
                24,
                50
            );

            _context.SportCenters.Add(sportCenter);
            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsOwnedByUserAsync(courtId.Value, otherUserId);

            // Assert
            Assert.False(result);
        }
    }
}
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using CourtBooking.Infrastructure.Data;
using CourtBooking.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CourtBooking.Test.Infrastructure.Data;
using System.Text.Json;
using System.Collections.Generic;

namespace CourtBooking.Test.Application.Repositories
{
    [Collection("PostgresDatabase")]
    public class CourtRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ICourtRepository _repository;
        private readonly PostgresTestFixture _fixture;

        public CourtRepositoryTests(PostgresTestFixture fixture)
        {
            _fixture = fixture;
            _context = new ApplicationDbContext(_fixture.ContextOptions);
            _repository = new CourtRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
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
                CourtName.Of("Tennis Court 1"),
                sportCenterId,
                sportId,
                TimeSpan.FromHours(1),
                "Main Court",
                "Indoor",
                CourtType.Indoor,
                50
            );

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCourtByIdAsync(courtId, CancellationToken.None);

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
            var result = await _repository.GetCourtByIdAsync(nonExistingId, CancellationToken.None);

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
            var ownerId = OwnerId.Of(Guid.NewGuid());

            var sportCenter = SportCenter.Create(
                sportCenterId,
                ownerId,
                "Sport Center 1",
                "123456789",
                new Location("Address", "City", "Country", "10000"),
                new GeoLocation(10.0, 20.0),
                new SportCenterImages("main.jpg", new System.Collections.Generic.List<string>()),
                "Description"
            );

            var court = Court.Create(
                courtId,
                CourtName.Of("Tennis Court 1"),
                sportCenterId,
                sportId,
                TimeSpan.FromHours(1),
                "Main Court",
                "Indoor",
                CourtType.Indoor,
                50
            );

            _context.SportCenters.Add(sportCenter);
            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsOwnedByUserAsync(courtId.Value, ownerId.Value, CancellationToken.None);

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
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var otherUserId = Guid.NewGuid();

            var sportCenter = SportCenter.Create(
                sportCenterId,
                ownerId,
                "Sport Center 1",
                "123456789",
                new Location("Address", "City", "Country", "10000"),
                new GeoLocation(10.0, 20.0),
                new SportCenterImages("main.jpg", new System.Collections.Generic.List<string>()),
                "Description"
            );

            var court = Court.Create(
                courtId,
                CourtName.Of("Tennis Court 1"),
                sportCenterId,
                sportId,
                TimeSpan.FromHours(1),
                "Main Court",
                "Indoor",
                CourtType.Indoor,
                50
            );

            _context.SportCenters.Add(sportCenter);
            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsOwnedByUserAsync(courtId.Value, otherUserId, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        private async Task<Court> CreateTestCourt()
        {
            var sport = await CreateTestSport();
            var sportcenter = await CreateTestSportCenter();

            var facilities = new List<SportCenterFacility>
            {
                new SportCenterFacility { Name = "Locker", Description = "Locker room" },
                new SportCenterFacility { Name = "Shower", Description = "Shower room" }
            };

            var court = Court.Create(
                CourtId.Of(Guid.NewGuid()),
                new CourtName("Test Court"),
                sportcenter.Id,
                sport.Id,
                TimeSpan.FromMinutes(60),
                "Description",
                JsonSerializer.Serialize(facilities),
                CourtType.Outdoor,
                50
            );

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            return court;
        }
    }
}
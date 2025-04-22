using CourtBooking.Application.Data;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Infrastructure.Data.Repositories;
using CourtBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CourtBooking.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using CourtBooking.Test.Infrastructure.Data;

namespace CourtBooking.Test.Application.Repositories
{
    [Collection("PostgresDatabase")]
    public class CourtPromotionRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CourtPromotionRepository _repository;
        private readonly PostgresTestFixture _fixture;

        public CourtPromotionRepositoryTests(PostgresTestFixture fixture)
        {
            _fixture = fixture;
            _context = new ApplicationDbContext(_fixture.ContextOptions);
            _repository = new CourtPromotionRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task AddAsync_Should_PersistPromotion()
        {
            // Arrange
            var court = await CreateTestCourt();
            var promotion = CreateTestPromotion(court.Id);

            // Act
            await _repository.AddAsync(promotion, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var savedPromotion = await _context.CourtPromotions.FirstOrDefaultAsync();
            Assert.NotNull(savedPromotion);
            Assert.NotNull(savedPromotion.CourtId);
            Assert.Equal(promotion.DiscountType, savedPromotion.DiscountType);
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnCorrectEntity()
        {
            // Arrange
            var court = await CreateTestCourt();
            var expected = CreateTestPromotion(court.Id);
            _context.CourtPromotions.Add(expected);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(expected.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Id.Value, result.Id.Value);
        }

        [Fact]
        public async Task UpdateAsync_Should_ModifyExistingPromotion()
        {
            // Arrange
            var court = await CreateTestCourt();
            var original = CreateTestPromotion(court.Id, discountValue: 15m);
            _context.CourtPromotions.Add(original);
            await _context.SaveChangesAsync();

            var updatedDiscount = 20m;
            original.Update(original.Description, original.DiscountType, updatedDiscount, original.ValidFrom, original.ValidTo);

            // Act
            await _repository.UpdateAsync(original, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var updatedPromotion = await _context.CourtPromotions.FindAsync(original.Id);
            Assert.NotNull(updatedPromotion);
            Assert.Equal(updatedDiscount, updatedPromotion.DiscountValue);
        }

        [Fact]
        public async Task DeleteAsync_Should_RemovePromotion()
        {
            // Arrange
            var court = await CreateTestCourt();
            var promotion = CreateTestPromotion(court.Id);
            _context.CourtPromotions.Add(promotion);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(promotion.Id, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.CourtPromotions.FindAsync(promotion.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPromotionsByCourtIdAsync_Should_FilterCorrectly()
        {
            // Arrange
            var court1 = await CreateTestCourt();
            var court2 = await CreateTestCourt();
            var validFrom = DateTime.UtcNow;
            var validTo = validFrom.AddDays(7);
            var promotion1 = CreateTestPromotion(court1.Id, validFrom: validFrom, validTo: validTo);
            var promotion2 = CreateTestPromotion(court2.Id, "FIXED", 20m, validFrom, validTo);
            await _repository.AddAsync(promotion1, CancellationToken.None);
            await _repository.AddAsync(promotion2, CancellationToken.None);

            // Act
            var result = await _repository.GetPromotionsByCourtIdAsync(court1.Id, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(promotion1.Id, result.First().Id);
        }

        [Fact]
        public async Task GetValidPromotionsForCourtAsync_Should_ReturnActivePromotions()
        {
            // Arrange
            var court = await CreateTestCourt();
            var currentDate = new DateTime(2025, 03, 18);

            var promotions = new List<CourtPromotion>
            {
                CreateTestPromotion(court.Id, validFrom: currentDate.AddDays(-5), validTo: currentDate.AddDays(5)),  // Active
                CreateTestPromotion(court.Id, validFrom: currentDate.AddDays(-1), validTo: currentDate.AddDays(1)),   // Active
                CreateTestPromotion(court.Id, validFrom: currentDate.AddDays(-10), validTo: currentDate.AddDays(-5)), // Expired
                CreateTestPromotion(court.Id, validFrom: currentDate.AddDays(5), validTo: currentDate.AddDays(10))     // Future
            };

            _context.CourtPromotions.AddRange(promotions);
            await _context.SaveChangesAsync();

            // Act
            var results = await _repository.GetValidPromotionsForCourtAsync(
                court.Id, currentDate, currentDate, CancellationToken.None);

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Contains(results, p => p.ValidFrom <= currentDate && p.ValidTo >= currentDate);
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

        private CourtPromotion CreateTestPromotion(CourtId courtId, string discountType = "PERCENTAGE", decimal discountValue = 10m, DateTime? validFrom = null, DateTime? validTo = null)
        {
            return CourtPromotion.Create(
                courtId,
                "Test Promotion",
                discountType,
                discountValue,
                validFrom ?? DateTime.UtcNow,
                validTo ?? DateTime.UtcNow.AddDays(7)
            );
        }

        [Fact]
        public async Task AddPromotionAsync_Should_PersistData()
        {
            // Arrange
            var court = await CreateTestCourt();
            var validFrom = DateTime.UtcNow;
            var validTo = validFrom.AddDays(7);
            var promotion = CreateTestPromotion(court.Id, "PERCENTAGE", 10m, validFrom, validTo);

            // Act
            await _repository.AddAsync(promotion, CancellationToken.None);

            // Assert
            var savedPromotion = await _context.CourtPromotions.FirstAsync();
            Assert.NotNull(savedPromotion);
            Assert.Equal("PERCENTAGE", savedPromotion.DiscountType);
            Assert.Equal(10m, savedPromotion.DiscountValue);
        }

        [Fact]
        public async Task GetPromotionByIdAsync_Should_ReturnCorrectEntity()
        {
            // Arrange
            var court = await CreateTestCourt();
            var validFrom = DateTime.UtcNow;
            var validTo = validFrom.AddDays(7);
            var promotion = CreateTestPromotion(court.Id, "PERCENTAGE", 10m, validFrom, validTo);
            await _repository.AddAsync(promotion, CancellationToken.None);

            // Act
            var result = await _repository.GetByIdAsync(promotion.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(promotion.CourtId, result.CourtId);
            Assert.Equal("PERCENTAGE", result.DiscountType);
            Assert.Equal(10m, result.DiscountValue);
        }

        [Fact]
        public async Task UpdatePromotionAsync_Should_ModifyExistingEntity()
        {
            // Arrange
            var court = await CreateTestCourt();
            var promotion = CreateTestPromotion(court.Id);
            await _repository.AddAsync(promotion, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Act
            var newDiscountValue = 20m;
            promotion.Update(promotion.Description, promotion.DiscountType, newDiscountValue, promotion.ValidFrom, promotion.ValidTo);
            await _repository.UpdateAsync(promotion, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var updatedPromotion = await _context.CourtPromotions.FindAsync(promotion.Id);
            Assert.NotNull(updatedPromotion);
            Assert.Equal(newDiscountValue, updatedPromotion.DiscountValue);
        }

        [Fact]
        public async Task DeletePromotionAsync_Should_RemoveEntity()
        {
            // Arrange
            var court = await CreateTestCourt();
            var promotion = CreateTestPromotion(court.Id);
            await _repository.AddAsync(promotion, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(promotion.Id, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var deletedPromotion = await _context.CourtPromotions.FindAsync(promotion.Id);
            Assert.Null(deletedPromotion);
        }
    }
}
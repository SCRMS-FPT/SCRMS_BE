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

namespace CourtBooking.Test.Application.Repositories
{
    public class CourtPromotionRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CourtPromotionRepository _repository;

        private const string ConnectionString = "Host=localhost;Port=5432;Database=courtbooking_test;Username=postgres;Password=123456";

        public CourtPromotionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new CourtPromotionRepository(_context);

            // Dọn dẹp dữ liệu trước mỗi test
            _context.CourtPromotions.RemoveRange(_context.CourtPromotions);
            _context.Courts.RemoveRange(_context.Courts);
            _context.Sports.RemoveRange(_context.Sports);
            _context.SportCenters.RemoveRange(_context.SportCenters);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task AddAsync_Should_PersistPromotion()
        {
            // Arrange
            var promotion = CreateTestPromotion();

            // Act
            await _repository.AddAsync(promotion, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Single(_context.CourtPromotions);
            Assert.Equal(promotion.DiscountType, _context.CourtPromotions.First().DiscountType);
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnCorrectEntity()
        {
            // Arrange
            var expected = CreateTestPromotion();
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
            var original = CreateTestPromotion(discountValue: 15m);
            _context.CourtPromotions.Add(original);
            await _context.SaveChangesAsync();

            var updatedDiscount = 20m;
            original.Update(original.Description, original.DiscountType, updatedDiscount, original.ValidFrom, original.ValidTo);

            // Act
            await _repository.UpdateAsync(original, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var updatedPromotion = _context.CourtPromotions.Find(original.Id);
            Assert.NotNull(updatedPromotion);
            Assert.Equal(updatedDiscount, updatedPromotion.DiscountValue);
        }

        [Fact]
        public async Task DeleteAsync_Should_RemovePromotion()
        {
            // Arrange
            var promotion = CreateTestPromotion();
            _context.CourtPromotions.Add(promotion);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(promotion.Id, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Empty(_context.CourtPromotions);
        }

        [Fact]
        public async Task GetPromotionsByCourtIdAsync_Should_FilterCorrectly()
        {
            // Arrange
            var court = CreateTestCourt(); // Đảm bảo court tồn tại
            var testData = new List<CourtPromotion>
            {
                CreateTestPromotion(court.Id),
                CreateTestPromotion(court.Id),
                CreateTestPromotion()
            };

            _context.CourtPromotions.AddRange(testData);
            await _context.SaveChangesAsync();

            // Act
            var results = await _repository.GetPromotionsByCourtIdAsync(court.Id, CancellationToken.None);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, p => Assert.Equal(court.Id, p.CourtId));
        }

        [Fact]
        public async Task GetValidPromotionsForCourtAsync_Should_ReturnActivePromotions()
        {
            // Arrange
            var court = CreateTestCourt();
            var currentDate = new DateTime(2025, 03, 18);

            var testData = new List<CourtPromotion>
            {
                CreateTestPromotionWithDates(court.Id, currentDate.AddDays(-5), currentDate.AddDays(5)),  // Active
                CreateTestPromotionWithDates(court.Id, currentDate.AddDays(-1), currentDate.AddDays(1)),   // Active
                CreateTestPromotionWithDates(court.Id, currentDate.AddDays(-10), currentDate.AddDays(-5)), // Expired
                CreateTestPromotionWithDates(court.Id, currentDate.AddDays(5), currentDate.AddDays(10))     // Future
            };

            _context.CourtPromotions.AddRange(testData);
            await _context.SaveChangesAsync();

            // Act
            var results = await _repository.GetValidPromotionsForCourtAsync(
                court.Id, currentDate, currentDate, CancellationToken.None);

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Contains(results, p => p.ValidFrom <= currentDate && p.ValidTo >= currentDate);
        }

        private Sport CreateTestSport()
        {
            var sport = Sport.Create(
                SportId.Of(Guid.NewGuid()),
                "Test Sport",
                "Test Sport Description",
                "icon.png"
            );

            _context.Sports.Add(sport);
            _context.SaveChanges();

            return sport;
        }

        private SportCenter CreateTestSportCenter()
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

            _context.SportCenters.Add(sportCenter);
            _context.SaveChanges();

            return sportCenter;
        }

        private Court CreateTestCourt()
        {
            var sport = CreateTestSport();
            var sportcenter = CreateTestSportCenter();

            var court = Court.Create(
                CourtId.Of(Guid.NewGuid()),
                new CourtName("Test Court"),
                sportcenter.Id,
                sport.Id,
                TimeSpan.FromMinutes(60),
                "Description",
                "[]", // Sửa thành JSON array rỗng
                CourtType.Outdoor,
                50
            );

            _context.Courts.Add(court);
            _context.SaveChanges();

            return court;
        }

        private CourtPromotion CreateTestPromotion(CourtId? courtId = null, string discountType = "Percentage",
            decimal discountValue = 15m, DateTime? validFrom = null, DateTime? validTo = null)
        {
            var court = courtId != null ? _context.Courts.Find(courtId.Value) : CreateTestCourt();

            if (court == null)
            {
                throw new Exception("Court must exist before creating a promotion.");
            }

            return CourtPromotion.Create(
                court.Id,
                "Test Promotion",
                discountType,
                discountValue,
                validFrom ?? DateTime.Today,
                validTo ?? DateTime.Today.AddDays(15)
            );
        }

        private CourtPromotion CreateTestPromotionWithDates(CourtId courtId, DateTime validFrom, DateTime validTo)
        {
            return CourtPromotion.Create(
                courtId,
                "Test Promotion with Dates",
                "Percentage",
                25m,
                validFrom,
                validTo
            );
        }
    }
}
using Coach.API.Data.Repositories;
using Coach.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Coach.API.Data.Models;

namespace Coach.API.Tests.Repositories
{
    public class CoachPromotionRepositoryTests
    {
        private readonly CoachPromotionRepository _repository;
        private readonly CoachDbContext _context;

        public CoachPromotionRepositoryTests()
        {
            // Sử dụng cơ sở dữ liệu trong bộ nhớ
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Tạo cơ sở dữ liệu trong bộ nhớ
                          .Options;
            _context = new CoachDbContext(options);
            _repository = new CoachPromotionRepository(_context);
        }

        [Fact]
        public async Task AddCoachPromotionAsync_ValidPromotion_AddsPromotion()
        {
            // Arrange
            var promotion = new CoachPromotion
            {
                Id = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                Description = "Test Promotion",
                DiscountType = "Percentage",
                DiscountValue = 10.00m,
                ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                ValidTo = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            };

            // Act
            await _repository.AddCoachPromotionAsync(promotion, CancellationToken.None);

            // Assert
            var addedPromotion = await _context.CoachPromotions.FindAsync(promotion.Id);
            Assert.NotNull(addedPromotion);
            Assert.Equal(promotion.Id, addedPromotion.Id);
        }

        [Fact]
        public async Task AddCoachPromotionAsync_InvalidPromotion_ThrowsException()
        {
            // Arrange
            var promotion = new CoachPromotion { Id = Guid.NewGuid() }; // Missing CoachId and DiscountType

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddCoachPromotionAsync(promotion, CancellationToken.None));

            // Assert that both validation errors are included in the exception message
            Assert.Contains("CoachId is required", exception.Message);
            Assert.Contains("DiscountType is required", exception.Message);
        }

        [Fact]
        public async Task GetCoachPromotionByIdAsync_ExistingId_ReturnsPromotion()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var promotion = new CoachPromotion
            {
                Id = promotionId,
                CoachId = Guid.NewGuid(),
                Description = "Test Promotion",  // Add a valid Description
                DiscountType = "Percentage"     // Add a valid DiscountType
            };
            await _context.CoachPromotions.AddAsync(promotion);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachPromotionByIdAsync(promotionId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(promotionId, result.Id);
        }

        [Fact]
        public async Task GetCoachPromotionByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var promotionId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachPromotionByIdAsync(promotionId, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCoachPromotionAsync_ExistingPromotion_UpdatesPromotion()
        {
            // Arrange
            var promotion = new CoachPromotion
            {
                Id = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                Description = "Updated Promotion",
                DiscountValue = 15.00m,
                DiscountType = "Percentage" // Add a valid value for DiscountType
            };
            await _context.CoachPromotions.AddAsync(promotion);
            await _context.SaveChangesAsync();

            // Thay đổi thông tin promotion
            promotion.Description = "Updated Description";
            promotion.DiscountValue = 20.00m;
            promotion.DiscountType = "Flat"; // Ensure to update DiscountType if needed

            // Act
            await _repository.UpdateCoachPromotionAsync(promotion, CancellationToken.None);

            // Assert
            var updatedPromotion = await _context.CoachPromotions.FindAsync(promotion.Id);
            Assert.NotNull(updatedPromotion);
            Assert.Equal("Updated Description", updatedPromotion.Description);
            Assert.Equal(20.00m, updatedPromotion.DiscountValue);
            Assert.Equal("Flat", updatedPromotion.DiscountType); // Assert that DiscountType is updated correctly
        }

        [Fact]
        public async Task UpdateCoachPromotionAsync_NonExistingPromotion_ThrowsException()
        {
            // Arrange
            var promotion = new CoachPromotion { Id = Guid.NewGuid(), CoachId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateCoachPromotionAsync(promotion, CancellationToken.None));
        }

        [Fact]
        public async Task GetCoachPromotionsByCoachIdAsync_CoachWithPromotions_ReturnsPromotions()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var promotions = new List<CoachPromotion>
            {
                new CoachPromotion { Id = Guid.NewGuid(), CoachId = coachId ,
                Description = "Test Promotion",  // Add a valid Description
                DiscountType = "Percentage" },
                new CoachPromotion { Id = Guid.NewGuid(), CoachId = coachId ,
                Description = "Test Promotion 2",  // Add a valid Description
                DiscountType = "Percentage" }
            };
            await _context.CoachPromotions.AddRangeAsync(promotions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachPromotionsByCoachIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(coachId, p.CoachId));
        }

        [Fact]
        public async Task GetCoachPromotionsByCoachIdAsync_CoachWithoutPromotions_ReturnsEmptyList()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachPromotionsByCoachIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }
}
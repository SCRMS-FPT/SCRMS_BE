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
    public class CoachPackagePurchaseRepositoryTests
    {
        private readonly CoachPackagePurchaseRepository _repository;
        private readonly CoachDbContext _context;

        public CoachPackagePurchaseRepositoryTests()
        {
            // Sử dụng cơ sở dữ liệu trong bộ nhớ
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Tạo cơ sở dữ liệu trong bộ nhớ
                          .Options;
            _context = new CoachDbContext(options);
            _repository = new CoachPackagePurchaseRepository(_context);
        }

        [Fact]
        public async Task AddCoachPackagePurchaseAsync_ValidPurchase_AddsPurchase()
        {
            // Arrange
            var purchase = new CoachPackagePurchase
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CoachPackageId = Guid.NewGuid(),
                PurchaseDate = DateTime.UtcNow,
                SessionsUsed = 0,
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };

            // Act
            await _repository.AddCoachPackagePurchaseAsync(purchase, CancellationToken.None);

            // Assert
            var addedPurchase = await _context.CoachPackagePurchases.FindAsync(purchase.Id);
            Assert.NotNull(addedPurchase);
            Assert.Equal(purchase.Id, addedPurchase.Id);
        }

        [Fact]
        public async Task AddCoachPackagePurchaseAsync_InvalidUserIdPurchase_ThrowsException()
        {
            // Arrange
            var purchase = new CoachPackagePurchase { Id = Guid.NewGuid() }; // Missing UserId, CoachPackageId

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddCoachPackagePurchaseAsync(purchase, CancellationToken.None));

            // Verify the exception message (optional, but it helps to understand the failure)
            Assert.Contains("UserId is required", exception.Message);
        }

        [Fact]
        public async Task AddCoachPackagePurchaseAsync_InvalidCoachPackageIdPurchase_ThrowsException()
        {
            // Arrange
            var purchase = new CoachPackagePurchase { Id = Guid.NewGuid(), UserId = Guid.NewGuid() }; // Missing CoachPackageId

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddCoachPackagePurchaseAsync(purchase, CancellationToken.None));

            // Verify the exception message (optional, but it helps to understand the failure)
            Assert.Contains("CoachPackageId is required", exception.Message);
        }

        [Fact]
        public async Task GetCoachPackagePurchaseByIdAsync_ExistingId_ReturnsPurchase()
        {
            // Arrange
            var purchaseId = Guid.NewGuid();
            var purchase = new CoachPackagePurchase { Id = purchaseId, UserId = Guid.NewGuid() };
            await _context.CoachPackagePurchases.AddAsync(purchase);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachPackagePurchaseByIdAsync(purchaseId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(purchaseId, result.Id);
        }

        [Fact]
        public async Task GetCoachPackagePurchaseByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var purchaseId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachPackagePurchaseByIdAsync(purchaseId, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCoachPackagePurchasesByUserIdAsync_UserWithPurchases_ReturnsPurchases()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var purchases = new List<CoachPackagePurchase>
            {
                new CoachPackagePurchase { Id = Guid.NewGuid(), UserId = userId },
                new CoachPackagePurchase { Id = Guid.NewGuid(), UserId = userId }
            };
            await _context.CoachPackagePurchases.AddRangeAsync(purchases);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachPackagePurchasesByUserIdAsync(userId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(userId, p.UserId));
        }

        [Fact]
        public async Task GetCoachPackagePurchasesByUserIdAsync_UserWithoutPurchases_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachPackagePurchasesByUserIdAsync(userId, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateCoachPackagePurchaseAsync_ExistingPurchase_UpdatesPurchase()
        {
            // Arrange
            var purchase = new CoachPackagePurchase
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CoachPackageId = Guid.NewGuid(),
                SessionsUsed = 1
            };
            await _context.CoachPackagePurchases.AddAsync(purchase);
            await _context.SaveChangesAsync();

            // Thay đổi thông tin purchase
            purchase.SessionsUsed = 2;

            // Act
            await _repository.UpdateCoachPackagePurchaseAsync(purchase, CancellationToken.None);

            // Assert
            var updatedPurchase = await _context.CoachPackagePurchases.FindAsync(purchase.Id);
            Assert.NotNull(updatedPurchase);
            Assert.Equal(2, updatedPurchase.SessionsUsed);
        }

        [Fact]
        public async Task UpdateCoachPackagePurchaseAsync_NonExistingPurchase_ThrowsException()
        {
            // Arrange
            var purchase = new CoachPackagePurchase { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateCoachPackagePurchaseAsync(purchase, CancellationToken.None));

            // Optionally, verify the exception message
            Assert.Contains("The purchase does not exist", exception.Message);
        }
    }
}
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Data.Models;
using Payment.API.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payment.API.Tests.Repositories
{
    public class WalletTransactionRepositoryTests
    {
        private readonly PaymentDbContext _context;
        private readonly IWalletTransactionRepository _repository;
        private readonly SqliteConnection _connection;

        public WalletTransactionRepositoryTests()
        {
            // Tạo kết nối SQLite in-memory
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new PaymentDbContext(options);
            // Tạo schema cho database
            _context.Database.EnsureCreated();

            _repository = new WalletTransactionRepository(_context);
        }

        [Fact]
        public async Task AddWalletTransactionAsync_AddsTransaction_WhenValid()
        {
            // Arrange
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TransactionType = "DEPOSIT",
                Amount = 50m,
                Description = "Test Deposit",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _repository.AddWalletTransactionAsync(transaction, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var addedTransaction = await _context.WalletTransactions.FindAsync(transaction.Id);
            Assert.NotNull(addedTransaction);
            Assert.Equal(transaction.Amount, addedTransaction.Amount);
        }

        [Fact]
        public async Task AddWalletTransactionAsync_ThrowsException_WhenIdExists()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var transaction = new WalletTransaction
            {
                Id = transactionId,
                UserId = Guid.NewGuid(),
                TransactionType = "DEPOSIT",
                Amount = 50m,
                CreatedAt = DateTime.UtcNow
            };
            await _context.WalletTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            _context.Entry(transaction).State = EntityState.Detached;

            var duplicateTransaction = new WalletTransaction
            {
                Id = transactionId,
                UserId = Guid.NewGuid(),
                TransactionType = "WITHDRAW",
                Amount = 20m,
                CreatedAt = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _repository.AddWalletTransactionAsync(duplicateTransaction, CancellationToken.None);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task GetTransactionsByUserIdAsync_ReturnsTransactions_WhenExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var transactions = new List<WalletTransaction>
            {
                new WalletTransaction { Id = Guid.NewGuid(), UserId = userId, TransactionType = "DEPOSIT", Amount = 50m, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                new WalletTransaction { Id = Guid.NewGuid(), UserId = userId, TransactionType = "WITHDRAW", Amount = 30m, CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                new WalletTransaction { Id = Guid.NewGuid(), UserId = userId, TransactionType = "DEPOSIT", Amount = 20m, CreatedAt = DateTime.UtcNow }
            };
            await _context.WalletTransactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTransactionsByUserIdAsync(userId, 1, 2, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(20m, result[0].Amount); // Sắp xếp giảm dần theo CreatedAt
            Assert.Equal(30m, result[1].Amount);
        }

        [Fact]
        public async Task GetTransactionsByUserIdAsync_ReturnsEmptyList_WhenNoTransactions()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.GetTransactionsByUserIdAsync(userId, 1, 10, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTransactionsByUserIdAsync_ReturnsEmptyList_WhenPageOutOfRange()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var transactions = new List<WalletTransaction>
            {
                new WalletTransaction { Id = Guid.NewGuid(), UserId = userId, TransactionType = "DEPOSIT", Amount = 50m, CreatedAt = DateTime.UtcNow }
            };
            await _context.WalletTransactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTransactionsByUserIdAsync(userId, 2, 10, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }
}
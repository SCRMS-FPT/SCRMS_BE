using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Data.Models;
using Payment.API.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payment.API.Tests.Repositories
{
    public class UserWalletRepositoryTests
    {
        private readonly PaymentDbContext _context;
        private readonly IUserWalletRepository _repository;
        private readonly SqliteConnection _connection;

        public UserWalletRepositoryTests()
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

            _repository = new UserWalletRepository(_context);
        }

        [Fact]
        public async Task GetUserWalletByUserIdAsync_ReturnsWallet_WhenWalletExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
            await _context.UserWallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserWalletByUserIdAsync(userId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(100m, result.Balance);
        }

        [Fact]
        public async Task GetUserWalletByUserIdAsync_ReturnsNull_WhenWalletDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.GetUserWalletByUserIdAsync(userId, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddUserWalletAsync_AddsWallet_WhenValid()
        {
            // Arrange
            var wallet = new UserWallet { UserId = Guid.NewGuid(), Balance = 50m, UpdatedAt = DateTime.UtcNow };

            // Act
            await _repository.AddUserWalletAsync(wallet, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var addedWallet = await _context.UserWallets.FindAsync(wallet.UserId);
            Assert.NotNull(addedWallet);
            Assert.Equal(wallet.Balance, addedWallet.Balance);
        }

        [Fact]
        public async Task AddUserWalletAsync_ThrowsException_WhenUserIdExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet
            {
                UserId = userId,
                Balance = 100m,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.UserWallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            // Detach wallet để EF Core không còn tracking nữa.
            _context.Entry(wallet).State = EntityState.Detached;

            var duplicateWallet = new UserWallet
            {
                UserId = userId,
                Balance = 200m,
                UpdatedAt = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _repository.AddUserWalletAsync(duplicateWallet, CancellationToken.None);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task UpdateUserWalletAsync_UpdatesWallet_WhenWalletExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
            await _context.UserWallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            wallet.Balance = 150m;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Act
            await _repository.UpdateUserWalletAsync(wallet, CancellationToken.None);
            await _context.SaveChangesAsync();

            // Assert
            var updatedWallet = await _context.UserWallets.FindAsync(userId);
            Assert.Equal(150m, updatedWallet.Balance);
        }

        [Fact]
        public async Task UpdateUserWalletAsync_ThrowsException_WhenWalletDoesNotExist()
        {
            // Arrange
            var wallet = new UserWallet { UserId = Guid.NewGuid(), Balance = 100m, UpdatedAt = DateTime.UtcNow };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await _repository.UpdateUserWalletAsync(wallet, CancellationToken.None);
                await _context.SaveChangesAsync();
            });
        }
    }
}
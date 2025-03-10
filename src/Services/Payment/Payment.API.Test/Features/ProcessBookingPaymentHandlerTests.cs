using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Payment.API.Data;
using Payment.API.Data.Models;
using Payment.API.Data.Repositories;
using Payment.API.Features.ProcessBookingPayment;
using Xunit;

namespace Payment.API.Tests.Features
{
    public class ProcessBookingPaymentHandlerTests
    {
        private readonly Mock<IUserWalletRepository> _userWalletRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTransactionRepoMock;
        private readonly PaymentDbContext _context;
        private readonly ProcessBookingPaymentHandler _handler;

        public ProcessBookingPaymentHandlerTests()
        {
            _userWalletRepoMock = new Mock<IUserWalletRepository>();
            _walletTransactionRepoMock = new Mock<IWalletTransactionRepository>();

            // Tạo DbContext với In-Memory Database, sử dụng tên database duy nhất cho mỗi test run.
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(databaseName: "ProcessBookingPaymentTestDb_" + Guid.NewGuid().ToString())
                .Options;
            _context = new PaymentDbContext(options);

            _handler = new ProcessBookingPaymentHandler(_userWalletRepoMock.Object, _walletTransactionRepoMock.Object, _context);
        }

        [Fact]
        public async Task Handle_ShouldProcessPayment_WhenBalanceIsSufficient()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);
            var command = new ProcessBookingPaymentCommand(userId, 50m, "Booking payment");

            // Act
            var transactionId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, transactionId);
            _userWalletRepoMock.Verify(r =>
                r.UpdateUserWalletAsync(It.Is<UserWallet>(w => w.Balance == 50m), It.IsAny<CancellationToken>()),
                Times.Once);
            _walletTransactionRepoMock.Verify(r =>
                r.AddWalletTransactionAsync(It.Is<WalletTransaction>(t => t.Amount == -50m), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenBalanceIsInsufficient()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 30m, UpdatedAt = DateTime.UtcNow };
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);
            var command = new ProcessBookingPaymentCommand(userId, 50m, "Booking payment");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Insufficient balance.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenAmountIsZeroOrNegative()
        {
            // Arrange
            var command = new ProcessBookingPaymentCommand(Guid.NewGuid(), 0m, "Booking payment");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Amount must be positive.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenWalletNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserWallet)null);
            var command = new ProcessBookingPaymentCommand(userId, 50m, "Booking payment");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Insufficient balance.", exception.Message);
        }
    }
}
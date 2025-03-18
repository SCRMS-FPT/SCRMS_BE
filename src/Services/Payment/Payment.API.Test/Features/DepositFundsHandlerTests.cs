using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Payment.API.Data;
using Payment.API.Data.Models;
using Payment.API.Data.Repositories;
using Payment.API.Features.DepositFunds;
using Xunit;
using BuildingBlocks.Messaging.Outbox;

namespace Payment.API.Tests.Features
{
    public class DepositFundsHandlerTests
    {
        private readonly Mock<IUserWalletRepository> _userWalletRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTransactionRepoMock;
        private readonly PaymentDbContext _context;
        private readonly DepositFundsHandler _handler;

        public DepositFundsHandlerTests()
        {
            _userWalletRepoMock = new Mock<IUserWalletRepository>();
            _walletTransactionRepoMock = new Mock<IWalletTransactionRepository>();

            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(databaseName: "DepositFundsTestDb_" + Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new PaymentDbContext(options);

            var outboxServiceMock = new Mock<IOutboxService>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new DepositFundsHandler(_userWalletRepoMock.Object, _walletTransactionRepoMock.Object, _context, outboxServiceMock.Object, unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldDepositFunds_WhenWalletExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var referenceId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);
            var command = new DepositFundsCommand(userId, 50m, referenceId, "TestPayment", "Test Description", null, null, null, null);

            // Act
            var transactionId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, transactionId);
            _userWalletRepoMock.Verify(r =>
                r.UpdateUserWalletAsync(It.Is<UserWallet>(w => w.Balance == 150m), It.IsAny<CancellationToken>()),
                Times.Once);

            _walletTransactionRepoMock.Verify(r =>
                r.AddWalletTransactionAsync(
                    It.Is<WalletTransaction>(t =>
                        t.ReferenceId == referenceId &&
                        t.Description.Contains(referenceId.ToString())),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateWalletAndDeposit_WhenWalletDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var referenceId = Guid.NewGuid();
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserWallet)null);
            var command = new DepositFundsCommand(userId, 50m, referenceId, "TestPayment", "Test Description", null, null, null, null);

            // Act
            var transactionId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, transactionId);
            _userWalletRepoMock.Verify(r =>
                r.AddUserWalletAsync(It.Is<UserWallet>(w =>
                    w.Balance == 50m &&
                    w.UserId == userId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldAcceptNullReference()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);
            var command = new DepositFundsCommand(userId, 50m, null, "TestPayment", "Test Description", null, null, null, null);

            // Act
            var transactionId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _walletTransactionRepoMock.Verify(r =>
                r.AddWalletTransactionAsync(
                    It.Is<WalletTransaction>(t =>
                        t.ReferenceId == null &&
                        t.Description == "Manual deposit"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenAmountIsZeroOrNegative()
        {
            // Arrange
            var command = new DepositFundsCommand(Guid.NewGuid(), 0m, Guid.NewGuid(), "TestPayment", "Test Description", null, null, null, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Amount must be positive.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldValidateReferenceIdFormat_WhenProvided()
        {
            // Arrange
            var invalidCommand = new DepositFundsCommand(Guid.NewGuid(), 100m, Guid.Empty, "TestPayment", "Test Description", null, null, null, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(invalidCommand, CancellationToken.None));
            Assert.Contains("Invalid reference format", exception.Message);
        }
    }
}
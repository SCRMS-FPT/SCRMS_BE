﻿using System;
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
        private readonly Mock<IOutboxService> _outboxServiceMock;
        private readonly DepositFundsHandler _handler;

        public DepositFundsHandlerTests()
        {
            _userWalletRepoMock = new Mock<IUserWalletRepository>();
            _walletTransactionRepoMock = new Mock<IWalletTransactionRepository>();
            _outboxServiceMock = new Mock<IOutboxService>();

            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(databaseName: "DepositFundsTestDb_" + Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new PaymentDbContext(options);

            _handler = new DepositFundsHandler(
                _userWalletRepoMock.Object,
                _walletTransactionRepoMock.Object,
                _context,
                _outboxServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldDepositFunds_WhenWalletExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);
            var command = new DepositFundsCommand(userId, 50m, "Test Description");

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
                        t.Description == "Test Description"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateWalletAndDeposit_WhenWalletDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserWallet)null);
            var command = new DepositFundsCommand(userId, 50m, "Test Description");

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
        public async Task Handle_ShouldAcceptNullDescription()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);
            var command = new DepositFundsCommand(userId, 50m, null);

            // Act
            var transactionId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _walletTransactionRepoMock.Verify(r =>
                r.AddWalletTransactionAsync(
                    It.Is<WalletTransaction>(t =>
                        t.Description == "Deposit funds"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenAmountIsZeroOrNegative()
        {
            // Arrange
            var command = new DepositFundsCommand(Guid.NewGuid(), 0m, "Test Description");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Amount must be positive.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldCreateCorrectTransactionType()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);
            var command = new DepositFundsCommand(userId, 50m, "Test Description");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _walletTransactionRepoMock.Verify(r =>
                r.AddWalletTransactionAsync(
                    It.Is<WalletTransaction>(t =>
                        t.TransactionType == "deposit"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
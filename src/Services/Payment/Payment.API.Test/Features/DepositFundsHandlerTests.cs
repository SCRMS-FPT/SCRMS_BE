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
using Microsoft.Extensions.Configuration;

namespace Payment.API.Tests.Features
{
    public class DepositFundsHandlerTests
    {
        private readonly Mock<IPendingDepositRepository> _pendingDepositRepoMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly DepositFundsHandler _handler;

        public DepositFundsHandlerTests()
        {
            _pendingDepositRepoMock = new Mock<IPendingDepositRepository>();
            _configurationMock = new Mock<IConfiguration>();

            // Set up configuration mock to return a value for Sepay:BankInfo
            _configurationMock.Setup(c => c.GetSection("Sepay:BankInfo").Value)
                .Returns("Test Bank Info");

            _handler = new DepositFundsHandler(
                _pendingDepositRepoMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreatePendingDeposit()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DepositFundsCommand(userId, 50m, "Test Description");

            _pendingDepositRepoMock
                .Setup(r => r.AddAsync(It.IsAny<PendingDeposit>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.DepositId);
            Assert.Equal(50m, result.Amount);
            Assert.StartsWith("ORD", result.DepositCode);
            Assert.Equal("Test Bank Info", result.BankInfo);

            _pendingDepositRepoMock.Verify(r =>
                r.AddAsync(
                    It.Is<PendingDeposit>(p =>
                        p.UserId == userId &&
                        p.Amount == 50m &&
                        p.Description == "Test Description" &&
                        p.Status == "Pending"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseDefaultDescription_WhenDescriptionIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DepositFundsCommand(userId, 50m, null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _pendingDepositRepoMock.Verify(r =>
                r.AddAsync(
                    It.Is<PendingDeposit>(p =>
                        p.Description == "Deposit funds"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldGenerateUniqueCode()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DepositFundsCommand(userId, 50m, "Test");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.StartsWith("ORD", result.DepositCode);
            Assert.True(result.DepositCode.Length > 3);
        }

        [Fact]
        public async Task Handle_ShouldUseBankInfoFromConfiguration()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DepositFundsCommand(userId, 50m, "Test");

            _configurationMock.Setup(c => c.GetSection("Sepay:BankInfo").Value)
                .Returns("Custom Bank Info");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Custom Bank Info", result.BankInfo);
        }
    }
}
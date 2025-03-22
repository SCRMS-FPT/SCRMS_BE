using Moq;
using Payment.API.Data.Models;
using Payment.API.Data.Repositories;
using Payment.API.Features.GetTransactionHistory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payment.API.Tests.Features
{
    public class GetTransactionHistoryHandlerTests
    {
        private readonly Mock<IWalletTransactionRepository> _walletTransactionRepoMock;
        private readonly GetTransactionHistoryHandler _handler;

        public GetTransactionHistoryHandlerTests()
        {
            _walletTransactionRepoMock = new Mock<IWalletTransactionRepository>();
            _handler = new GetTransactionHistoryHandler(_walletTransactionRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTransactions_WhenExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var transactions = new List<WalletTransaction>
            {
                new WalletTransaction { Id = Guid.NewGuid(), UserId = userId, Amount = 50m, TransactionType = "deposit", CreatedAt = DateTime.UtcNow }
            };
            _walletTransactionRepoMock.Setup(r => r.GetTransactionsByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(transactions);
            var query = new GetTransactionHistoryQuery(userId, 1, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(50m, result[0].Amount);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoTransactions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _walletTransactionRepoMock.Setup(r => r.GetTransactionsByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(new List<WalletTransaction>());
            var query = new GetTransactionHistoryQuery(userId, 1, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }
}
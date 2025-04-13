using Moq;
using Payment.API.Data.Models;
using Payment.API.Data.Repositories;
using Payment.API.Features.GetTransactionHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BuildingBlocks.Pagination;

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
            _walletTransactionRepoMock.Setup(r => r.GetTransactionCountByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var query = new GetTransactionHistoryQuery(userId, 1, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var dataList = result.Data.ToList();
            Assert.Single(dataList);
            Assert.Equal(50m, dataList[0].Amount);
            Assert.Equal(1, result.Count);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoTransactions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _walletTransactionRepoMock.Setup(r => r.GetTransactionsByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(new List<WalletTransaction>());
            _walletTransactionRepoMock.Setup(r => r.GetTransactionCountByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(0);
            var query = new GetTransactionHistoryQuery(userId, 1, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.Count);
        }
    }
}
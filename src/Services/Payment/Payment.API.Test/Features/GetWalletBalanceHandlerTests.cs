using Moq;
using Payment.API.Data.Models;
using Payment.API.Data.Repositories;
using Payment.API.Features.GetWalletBalance;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payment.API.Tests.Features
{
    public class GetWalletBalanceHandlerTests
    {
        private readonly Mock<IUserWalletRepository> _userWalletRepoMock;
        private readonly GetWalletBalanceHandler _handler;

        public GetWalletBalanceHandlerTests()
        {
            _userWalletRepoMock = new Mock<IUserWalletRepository>();
            _handler = new GetWalletBalanceHandler(_userWalletRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnWallet_WhenWalletExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new UserWallet { UserId = userId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
            var query = new GetWalletBalanceQuery(userId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100m, result.Balance);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenWalletNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userWalletRepoMock.Setup(r => r.GetUserWalletByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserWallet)null);
            var query = new GetWalletBalanceQuery(userId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
            Assert.Equal("Wallet not found", exception.Message);
        }
    }
}
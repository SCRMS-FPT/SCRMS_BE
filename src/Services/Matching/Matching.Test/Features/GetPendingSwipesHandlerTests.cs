using Matching.API.Data.Models;
using Matching.API.Data.Repositories;
using Matching.API.Features.PendingResponses.GetPendingSwipes;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matching.Test.Features
{
    public class GetPendingSwipesHandlerTests
    {
        private readonly Mock<ISwipeActionRepository> _swipeRepoMock;
        private readonly GetPendingSwipesHandler _handler;

        public GetPendingSwipesHandlerTests()
        {
            _swipeRepoMock = new Mock<ISwipeActionRepository>();
            _handler = new GetPendingSwipesHandler(_swipeRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoPendingSwipes()
        {
            _swipeRepoMock.Setup(m => m.GetPendingSwipesByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SwipeAction>());

            var result = await _handler.Handle(new GetPendingSwipesQuery(Guid.NewGuid()), CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ReturnsPendingSwipes_WhenExist()
        {
            var userId = Guid.NewGuid();
            var swipes = new List<SwipeAction> { new SwipeAction { SwiperId = Guid.NewGuid(), SwipedUserId = userId, Decision = "pending", CreatedAt = DateTime.UtcNow } };
            _swipeRepoMock.Setup(m => m.GetPendingSwipesByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(swipes);

            var result = await _handler.Handle(new GetPendingSwipesQuery(userId), CancellationToken.None);

            Assert.Single(result);
            Assert.Equal(swipes[0].SwiperId, result[0].SwiperId);
        }

        [Fact]
        public async Task Handle_ReturnsMultiplePendingSwipes()
        {
            var userId = Guid.NewGuid();
            var swipes = new List<SwipeAction>
            {
                new SwipeAction { SwiperId = Guid.NewGuid(), SwipedUserId = userId, Decision = "pending", CreatedAt = DateTime.UtcNow },
                new SwipeAction { SwiperId = Guid.NewGuid(), SwipedUserId = userId, Decision = "pending", CreatedAt = DateTime.UtcNow }
            };
            _swipeRepoMock.Setup(m => m.GetPendingSwipesByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(swipes);

            var result = await _handler.Handle(new GetPendingSwipesQuery(userId), CancellationToken.None);

            Assert.Equal(2, result.Count);
        }
    }
}

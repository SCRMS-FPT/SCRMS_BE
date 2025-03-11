using Matching.API.Data;
using Matching.API.Data.Models;
using Matching.API.Data.Repositories;
using Matching.API.Features.RespondToSwipe;
using Microsoft.EntityFrameworkCore;
using Moq;
using Match = Matching.API.Data.Models.Match;

namespace Matching.Test.Features
{
    public class RespondToSwipeHandlerTests
    {
        private readonly Mock<ISwipeActionRepository> _swipeRepoMock;
        private readonly Mock<IMatchRepository> _matchRepoMock;
        private readonly Mock<MatchingDbContext> _contextMock;
        private readonly RespondToSwipeHandler _handler;

        public RespondToSwipeHandlerTests()
        {
            _swipeRepoMock = new Mock<ISwipeActionRepository>();
            _matchRepoMock = new Mock<IMatchRepository>();

            // Mock DbContextOptions
            var options = new DbContextOptionsBuilder<MatchingDbContext>()
                .UseInMemoryDatabase("TestDB")
                .Options;

            // Create mock with constructor argument
            _contextMock = new Mock<MatchingDbContext>(options);

            _handler = new RespondToSwipeHandler(_swipeRepoMock.Object, _matchRepoMock.Object, _contextMock.Object);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenSwipeNotFound()
        {
            // Arrange
            _swipeRepoMock.Setup(m => m.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((SwipeAction)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(new RespondToSwipeCommand(Guid.NewGuid(), "accepted", Guid.NewGuid()), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenUnauthorized()
        {
            // Arrange
            var swipeAction = new SwipeAction { Id = Guid.NewGuid(), SwiperId = Guid.NewGuid(), SwipedUserId = Guid.NewGuid() };
            _swipeRepoMock.Setup(m => m.GetByIdAsync(swipeAction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(swipeAction);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(new RespondToSwipeCommand(swipeAction.Id, "accepted", Guid.NewGuid()), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Rejected_UpdatesSwipeOnly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var swipeAction = new SwipeAction { Id = Guid.NewGuid(), SwiperId = Guid.NewGuid(), SwipedUserId = userId, Decision = "pending" };
            _swipeRepoMock.Setup(m => m.GetByIdAsync(swipeAction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(swipeAction);
            _contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(new RespondToSwipeCommand(swipeAction.Id, "rejected", userId), CancellationToken.None);

            // Assert
            Assert.False(result.IsMatch);
            _swipeRepoMock.Verify(m => m.UpdateSwipeActionAsync(It.Is<SwipeAction>(sa => sa.Decision == "rejected"), It.IsAny<CancellationToken>()), Times.Once());
            _matchRepoMock.Verify(m => m.AddMatchAsync(It.IsAny<Match>(), It.IsAny<CancellationToken>()), Times.Never());
            _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handle_AcceptedWithReverse_CreatesMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var swiperId = Guid.NewGuid();
            var swipeAction = new SwipeAction { Id = Guid.NewGuid(), SwiperId = swiperId, SwipedUserId = userId, Decision = "pending" };
            var reverseSwipe = new SwipeAction { SwiperId = userId, SwipedUserId = swiperId, Decision = "pending" };

            _swipeRepoMock.Setup(m => m.GetByIdAsync(swipeAction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(swipeAction);
            _swipeRepoMock.Setup(m => m.GetBySwiperAndSwipedAsync(userId, swiperId, It.IsAny<CancellationToken>())).ReturnsAsync(reverseSwipe);
            _contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(new RespondToSwipeCommand(swipeAction.Id, "accepted", userId), CancellationToken.None);

            // Assert
            Assert.True(result.IsMatch);
            _swipeRepoMock.Verify(m => m.UpdateSwipeActionAsync(It.Is<SwipeAction>(sa => sa.Decision == "accepted"), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _matchRepoMock.Verify(m => m.AddMatchAsync(It.Is<Match>(m => m.InitiatorId == swiperId && m.MatchedUserId == userId), It.IsAny<CancellationToken>()), Times.Once());
            _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handle_AcceptedWithoutReverse_DoesNotCreateMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var swipeAction = new SwipeAction { Id = Guid.NewGuid(), SwiperId = Guid.NewGuid(), SwipedUserId = userId, Decision = "pending" };
            _swipeRepoMock.Setup(m => m.GetByIdAsync(swipeAction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(swipeAction);
            _swipeRepoMock.Setup(m => m.GetBySwiperAndSwipedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((SwipeAction)null);
            _contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(new RespondToSwipeCommand(swipeAction.Id, "accepted", userId), CancellationToken.None);

            // Assert
            Assert.False(result.IsMatch);
            _swipeRepoMock.Verify(m => m.UpdateSwipeActionAsync(It.IsAny<SwipeAction>(), It.IsAny<CancellationToken>()), Times.Once());
            _matchRepoMock.Verify(m => m.AddMatchAsync(It.IsAny<Match>(), It.IsAny<CancellationToken>()), Times.Never());
        }
    }

}

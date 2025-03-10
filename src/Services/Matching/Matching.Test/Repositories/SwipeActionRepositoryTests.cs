using Matching.API.Data;
using Matching.API.Data.Models;
using Matching.API.Data.Repositories;
using Matching.Test.Helper;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Matching.Test.Repositories
{
    public class SwipeActionRepositoryTests : HandlerTestBase
    {
        private readonly SwipeActionRepository _repository;

        public SwipeActionRepositoryTests() : base()
        {
            _repository = new SwipeActionRepository(Context);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSwipe_WhenExists()
        {
            // Arrange
            var swipeId = Guid.NewGuid();
            var swipe = new SwipeAction
            {
                Id = swipeId,
                SwiperId = Guid.NewGuid(),
                SwipedUserId = Guid.NewGuid(),
                Decision = "pending",
                CreatedAt = DateTime.UtcNow
            };
            await Context.SwipeActions.AddAsync(swipe);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(swipeId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(swipeId, result.Id);
            Assert.Equal("pending", result.Decision);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBySwiperAndSwipedAsync_ReturnsSwipe_WhenExists()
        {
            // Arrange
            var swiperId = Guid.NewGuid();
            var swipedUserId = Guid.NewGuid();
            var swipe = new SwipeAction
            {
                Id = Guid.NewGuid(),
                SwiperId = swiperId,
                SwipedUserId = swipedUserId,
                Decision = "pending",
                CreatedAt = DateTime.UtcNow
            };
            await Context.SwipeActions.AddAsync(swipe);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBySwiperAndSwipedAsync(swiperId, swipedUserId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(swiperId, result.SwiperId);
            Assert.Equal(swipedUserId, result.SwipedUserId);
        }

        [Fact]
        public async Task GetBySwiperAndSwipedAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetBySwiperAndSwipedAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPendingSwipesByUserIdAsync_ReturnsPendingSwipes()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var swipe1 = new SwipeAction
            {
                Id = Guid.NewGuid(),
                SwiperId = Guid.NewGuid(),
                SwipedUserId = userId,
                Decision = "pending",
                CreatedAt = DateTime.UtcNow
            };
            var swipe2 = new SwipeAction
            {
                Id = Guid.NewGuid(),
                SwiperId = Guid.NewGuid(),
                SwipedUserId = userId,
                Decision = "accepted",
                CreatedAt = DateTime.UtcNow
            };
            await Context.SwipeActions.AddRangeAsync(swipe1, swipe2);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPendingSwipesByUserIdAsync(userId, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("pending", result[0].Decision);
            Assert.Equal(swipe1.Id, result[0].Id);
        }

        [Fact]
        public async Task GetPendingSwipesByUserIdAsync_ReturnsEmpty_WhenNoPending()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var swipe = new SwipeAction
            {
                Id = Guid.NewGuid(),
                SwiperId = Guid.NewGuid(),
                SwipedUserId = userId,
                Decision = "accepted",
                CreatedAt = DateTime.UtcNow
            };
            await Context.SwipeActions.AddAsync(swipe);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPendingSwipesByUserIdAsync(userId, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddSwipeActionAsync_AddsSwipe()
        {
            // Arrange
            var swipe = new SwipeAction
            {
                Id = Guid.NewGuid(),
                SwiperId = Guid.NewGuid(),
                SwipedUserId = Guid.NewGuid(),
                Decision = "pending",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _repository.AddSwipeActionAsync(swipe, CancellationToken.None);
            await Context.SaveChangesAsync();

            // Assert
            var addedSwipe = await Context.SwipeActions.FindAsync(swipe.Id);
            Assert.NotNull(addedSwipe);
            Assert.Equal(swipe.Id, addedSwipe.Id);
            Assert.Equal("pending", addedSwipe.Decision);
        }

        [Fact]
        public async Task UpdateSwipeActionAsync_UpdatesSwipe()
        {
            // Arrange
            var swipe = new SwipeAction
            {
                Id = Guid.NewGuid(),
                SwiperId = Guid.NewGuid(),
                SwipedUserId = Guid.NewGuid(),
                Decision = "pending",
                CreatedAt = DateTime.UtcNow
            };
            await Context.SwipeActions.AddAsync(swipe);
            await Context.SaveChangesAsync();
            swipe.Decision = "accepted";

            // Act
            await _repository.UpdateSwipeActionAsync(swipe, CancellationToken.None);
            await Context.SaveChangesAsync();

            // Assert
            var updatedSwipe = await Context.SwipeActions.FindAsync(swipe.Id);
            Assert.NotNull(updatedSwipe);
            Assert.Equal("accepted", updatedSwipe.Decision);
        }
    }
}

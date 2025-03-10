using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.UpdateReview;

namespace Reviews.Test.Features
{
    public class UpdateReviewHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly UpdateReviewHandler _handler;

        public UpdateReviewHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _handler = new UpdateReviewHandler(_mockReviewRepository.Object);
        }
        [Fact]
        public async Task Handle_ValidUpdate_UpdatesRating()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var originalReview = new Review { Id = reviewId, ReviewerId = userId, Rating = 3 };
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(originalReview);
            var command = new UpdateReviewCommand(reviewId, userId, 4, "Updated");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(4, originalReview.Rating);
        }

        [Fact]
        public async Task Handle_ValidUpdate_UpdatesComment()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var originalReview = new Review { Id = reviewId, ReviewerId = userId, Comment = "Original" };
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(originalReview);
            var command = new UpdateReviewCommand(reviewId, userId, 3, "Updated comment");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Updated comment", originalReview.Comment);
        }

        [Fact]
        public async Task Handle_ValidUpdate_UpdatesTimestamp()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var originalReview = new Review { Id = reviewId, ReviewerId = userId, UpdatedAt = DateTime.UtcNow.AddHours(-1) };
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(originalReview);
            var command = new UpdateReviewCommand(reviewId, userId, 3, "Updated");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(originalReview.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task Handle_ValidUpdate_SavesChanges()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var originalReview = new Review { Id = reviewId, ReviewerId = userId };
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(originalReview);
            var command = new UpdateReviewCommand(reviewId, userId, 3, "Updated");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ReviewNotFound_ThrowsException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync((Review)null);
            var command = new UpdateReviewCommand(reviewId, userId, 3, "Comment");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Review not found or unauthorized", exception.Message);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UnauthorizedUser_ThrowsException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();
            var review = new Review { Id = reviewId, ReviewerId = reviewerId };
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);
            var command = new UpdateReviewCommand(reviewId, userId, 3, "Comment");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Review not found or unauthorized", exception.Message);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidRating_ThrowsArgumentException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var review = new Review { Id = reviewId, ReviewerId = userId };
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);
            var command = new UpdateReviewCommand(reviewId, userId, 6, "Comment"); // Rating 6 is invalid

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Rating must be between 1 and 5.", exception.Message);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

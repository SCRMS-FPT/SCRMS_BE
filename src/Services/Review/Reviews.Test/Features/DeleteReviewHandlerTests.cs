using Microsoft.Extensions.Logging;
using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.DeleteReview;

namespace Reviews.Test.Features
{
    public class DeleteReviewHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly Mock<ILogger<DeleteReviewHandler>> _mockLogger;
        private readonly DeleteReviewHandler _handler;

        public DeleteReviewHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _mockLogger = new Mock<ILogger<DeleteReviewHandler>>();
            _handler = new DeleteReviewHandler(_mockReviewRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ReviewDoesNotExist_ThrowsException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new DeleteReviewCommand(reviewId, userId, false);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync((Review)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
            _mockReviewRepository.Verify(r => r.RemoveReviewAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UserNotAuthorized_ThrowsException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid(); // Different from userId
            var review = new Review { Id = reviewId, ReviewerId = reviewerId };
            var command = new DeleteReviewCommand(reviewId, userId, false);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
            _mockReviewRepository.Verify(r => r.RemoveReviewAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReviewExistsAndUserAuthorized_DeletesReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var review = new Review { Id = reviewId, ReviewerId = userId };
            var command = new DeleteReviewCommand(reviewId, userId, false);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockReviewRepository.Verify(r => r.RemoveReviewAsync(review, It.IsAny<CancellationToken>()), Times.Once);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AdminCanDeleteAnyReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid(); // Different from adminId
            var review = new Review { Id = reviewId, ReviewerId = reviewerId };
            var command = new DeleteReviewCommand(reviewId, adminId, true);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockReviewRepository.Verify(r => r.RemoveReviewAsync(review, It.IsAny<CancellationToken>()), Times.Once);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

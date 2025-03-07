using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.DeleteReview;

namespace Reviews.Test.Features
{
    public class DeleteReviewHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly DeleteReviewHandler _handler;

        public DeleteReviewHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _handler = new DeleteReviewHandler(_mockReviewRepository.Object);
        }

        [Fact]
        public async Task Handle_ReviewDoesNotExist_ThrowsException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new DeleteReviewCommand(reviewId, userId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync((Review)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Review not found or unauthorized", exception.Message);
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
            var command = new DeleteReviewCommand(reviewId, userId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Review not found or unauthorized", exception.Message);
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
            var command = new DeleteReviewCommand(reviewId, userId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockReviewRepository.Verify(r => r.RemoveReviewAsync(review, It.IsAny<CancellationToken>()), Times.Once);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

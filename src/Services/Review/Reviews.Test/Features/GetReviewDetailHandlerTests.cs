using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.GetReviewDetail;

namespace Reviews.Test.Features
{
    public class GetReviewDetailHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly GetReviewDetailHandler _handler;

        public GetReviewDetailHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _handler = new GetReviewDetailHandler(_mockReviewRepository.Object);
        }
        [Fact]
        public async Task Handle_ReviewDoesNotExist_ThrowsException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var query = new GetReviewDetailQuery(reviewId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync((Review)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ReviewExists_ReturnsNonNullResponse()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { Id = reviewId };
            var query = new GetReviewDetailQuery(reviewId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Handle_ReviewExists_SetsCorrectId()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { Id = reviewId };
            var query = new GetReviewDetailQuery(reviewId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(review.Id, response.Id);
        }

        [Fact]
        public async Task Handle_ReviewExists_SetsCorrectReviewerId()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();
            var review = new Review { Id = reviewId, ReviewerId = reviewerId };
            var query = new GetReviewDetailQuery(reviewId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(review.ReviewerId, response.ReviewerId);
        }

        [Fact]
        public async Task Handle_ReviewExists_SetsCorrectSubjectType()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { Id = reviewId, SubjectType = "court" };
            var query = new GetReviewDetailQuery(reviewId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(review.SubjectType, response.SubjectType);
        }

        [Fact]
        public async Task Handle_ReviewExists_SetsCorrectSubjectId()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            var review = new Review { Id = reviewId, SubjectId = subjectId };
            var query = new GetReviewDetailQuery(reviewId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(review.SubjectId, response.SubjectId);
        }

        [Fact]
        public async Task Handle_ReviewExists_SetsCorrectRating()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { Id = reviewId, Rating = 5 };
            var query = new GetReviewDetailQuery(reviewId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(review.Rating, response.Rating);
        }

        [Fact]
        public async Task Handle_ReviewExists_SetsCorrectComment()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { Id = reviewId, Comment = "Excellent!" };
            var query = new GetReviewDetailQuery(reviewId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(review.Comment, response.Comment);
        }

        [Fact]
        public async Task Handle_ReviewExists_SetsCorrectCreatedAt()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var review = new Review { Id = reviewId, CreatedAt = createdAt };
            var query = new GetReviewDetailQuery(reviewId);
            _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(review.CreatedAt, response.CreatedAt);
        }
    }
}

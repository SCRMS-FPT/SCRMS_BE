using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.GetReviewReplies;

namespace Reviews.Test.Features
{
    public class GetReviewRepliesHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly GetReviewRepliesHandler _handler;

        public GetReviewRepliesHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _handler = new GetReviewRepliesHandler(_mockReviewRepository.Object);
        }
        [Fact]
        public async Task Handle_ReturnsNonNullResponse()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var replies = new List<ReviewReply>
            {
                new ReviewReply { Id = Guid.NewGuid(), ReviewId = reviewId }
            };
            var query = new GetReviewRepliesQuery(reviewId, 1, 2);
            _mockReviewRepository.Setup(r => r.GetReviewRepliesAsync(reviewId, 1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(replies);
            _mockReviewRepository.Setup(r => r.CountReviewRepliesAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectNumberOfReplies()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var replies = new List<ReviewReply>
            {
                new ReviewReply { Id = Guid.NewGuid(), ReviewId = reviewId },
                new ReviewReply { Id = Guid.NewGuid(), ReviewId = reviewId }
            };
            var query = new GetReviewRepliesQuery(reviewId, 1, 2);
            _mockReviewRepository.Setup(r => r.GetReviewRepliesAsync(reviewId, 1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(replies);
            _mockReviewRepository.Setup(r => r.CountReviewRepliesAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, response.Count);
        }

        [Fact]
        public async Task Handle_ReturnsRepliesWithCorrectIds()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var replies = new List<ReviewReply>
            {
                new ReviewReply { Id = Guid.NewGuid(), ReviewId = reviewId },
                new ReviewReply { Id = Guid.NewGuid(), ReviewId = reviewId }
            };
            var query = new GetReviewRepliesQuery(reviewId, 1, 2);
            _mockReviewRepository.Setup(r => r.GetReviewRepliesAsync(reviewId, 1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(replies);
            _mockReviewRepository.Setup(r => r.CountReviewRepliesAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(replies.Select(r => r.Id), response.Data.Select(r => r.Id));
        }

        [Fact]
        public async Task Handle_Pagination_ReturnsCorrectCount()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var replies = new List<ReviewReply>
            {
                new ReviewReply { Id = Guid.NewGuid(), ReviewId = reviewId },
                new ReviewReply { Id = Guid.NewGuid(), ReviewId = reviewId }
            };
            var paginatedReplies = replies.Skip(1).Take(1).ToList();
            var query = new GetReviewRepliesQuery(reviewId, 2, 1);
            _mockReviewRepository.Setup(r => r.GetReviewRepliesAsync(reviewId, 2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(paginatedReplies);
            _mockReviewRepository.Setup(r => r.CountReviewRepliesAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(response.Data);
        }

        [Fact]
        public async Task Handle_Pagination_ReturnsCorrectReplyId()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var replyId1 = Guid.NewGuid();
            var replyId2 = Guid.NewGuid();
            var replies = new List<ReviewReply>
            {
                new ReviewReply { Id = replyId1, ReviewId = reviewId },
                new ReviewReply { Id = replyId2, ReviewId = reviewId }
            };
            var paginatedReplies = replies.Skip(1).Take(1).ToList();
            var query = new GetReviewRepliesQuery(reviewId, 2, 1);
            _mockReviewRepository.Setup(r => r.GetReviewRepliesAsync(reviewId, 2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(paginatedReplies);
            _mockReviewRepository.Setup(r => r.CountReviewRepliesAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(replyId2, response.Data.First().Id);
        }

        [Fact]
        public async Task Handle_EmptyReplies_ReturnsEmptyList()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var query = new GetReviewRepliesQuery(reviewId, 1, 10);
            _mockReviewRepository.Setup(r => r.GetReviewRepliesAsync(reviewId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ReviewReply>());
            _mockReviewRepository.Setup(r => r.CountReviewRepliesAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(0);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(response.Data);
            Assert.Equal(0, response.Count);
        }
    }
}

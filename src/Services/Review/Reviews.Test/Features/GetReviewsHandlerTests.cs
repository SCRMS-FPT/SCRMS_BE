using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.GetReviews;

namespace Reviews.Test.Features
{
    public class GetReviewsHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly GetReviewsHandler _handler;

        public GetReviewsHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _handler = new GetReviewsHandler(_mockReviewRepository.Object);
        }
        [Fact]
        public async Task Handle_ReturnsNonNullResponse()
        {
            // Arrange
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            var reviews = new List<Review> { new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId } };
            var query = new GetReviewsQuery(subjectType, subjectId, 1, 2);
            _mockReviewRepository.Setup(r => r.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(reviews);
            _mockReviewRepository.Setup(r => r.CountReviewsBySubjectAsync(subjectType, subjectId, It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectNumberOfReviews()
        {
            // Arrange
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId },
                new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId }
            };
            var query = new GetReviewsQuery(subjectType, subjectId, 1, 2);
            _mockReviewRepository.Setup(r => r.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(reviews);
            _mockReviewRepository.Setup(r => r.CountReviewsBySubjectAsync(subjectType, subjectId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, response.Count);
        }

        [Fact]
        public async Task Handle_ReturnsReviewsWithCorrectIds()
        {
            // Arrange
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId },
                new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId }
            };
            var query = new GetReviewsQuery(subjectType, subjectId, 1, 2);
            _mockReviewRepository.Setup(r => r.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(reviews);
            _mockReviewRepository.Setup(r => r.CountReviewsBySubjectAsync(subjectType, subjectId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(reviews.Select(r => r.Id), response.Data.Select(r => r.Id));
        }

        [Fact]
        public async Task Handle_Pagination_ReturnsCorrectCount()
        {
            // Arrange
            var subjectType = "coach";
            var subjectId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId },
                new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId }
            };
            var paginatedReviews = reviews.Skip(1).Take(1).ToList();
            var query = new GetReviewsQuery(subjectType, subjectId, 2, 1);
            _mockReviewRepository.Setup(r => r.GetReviewsBySubjectAsync(subjectType, subjectId, 2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(paginatedReviews);
            _mockReviewRepository.Setup(r => r.CountReviewsBySubjectAsync(subjectType, subjectId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(response.Data);
        }

        [Fact]
        public async Task Handle_Pagination_ReturnsCorrectReviewId()
        {
            // Arrange
            var subjectType = "coach";
            var subjectId = Guid.NewGuid();
            var reviewId1 = Guid.NewGuid();
            var reviewId2 = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { Id = reviewId1, SubjectType = subjectType, SubjectId = subjectId },
                new Review { Id = reviewId2, SubjectType = subjectType, SubjectId = subjectId }
            };
            var paginatedReviews = reviews.Skip(1).Take(1).ToList();
            var query = new GetReviewsQuery(subjectType, subjectId, 2, 1);
            _mockReviewRepository.Setup(r => r.GetReviewsBySubjectAsync(subjectType, subjectId, 2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(paginatedReviews);
            _mockReviewRepository.Setup(r => r.CountReviewsBySubjectAsync(subjectType, subjectId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(reviewId2, response.Data.First().Id);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var subjectType = "coach";
            var subjectId = Guid.NewGuid();
            var query = new GetReviewsQuery(subjectType, subjectId, 1, 10);
            _mockReviewRepository.Setup(r => r.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Review>());
            _mockReviewRepository.Setup(r => r.CountReviewsBySubjectAsync(subjectType, subjectId, It.IsAny<CancellationToken>())).ReturnsAsync(0);

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(response.Data);
            Assert.Equal(0, response.Count);
        }
    }
}

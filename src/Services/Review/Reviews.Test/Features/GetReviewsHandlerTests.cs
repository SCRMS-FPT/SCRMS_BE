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

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(reviews.Select(r => r.Id), response.Select(r => r.Id));
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

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(response);
        }

        [Fact]
        public async Task Handle_Pagination_ReturnsCorrectReviewId()
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

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(paginatedReviews[0].Id, response[0].Id);
        }
    }
}

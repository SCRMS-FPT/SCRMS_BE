﻿using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.GetSelfReviews;

namespace Reviews.Test.Features
{
    public class GetSelfReviewsHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly GetSelfReviewsHandler _handler;

        public GetSelfReviewsHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _handler = new GetSelfReviewsHandler(_mockReviewRepository.Object);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectNumberOfReviews()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { Id = Guid.NewGuid(), SubjectId = coachId },
                new Review { Id = Guid.NewGuid(), SubjectId = coachId }
            };
            _mockReviewRepository.Setup(r => r.GetReviewsByCoachIdAsync(coachId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(reviews);
            _mockReviewRepository.Setup(r => r.CountReviewsByCoachIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(2);
            var query = new GetSelfReviewsQuery(coachId, 1, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectReviewId()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var reviewId = Guid.NewGuid();
            var reviews = new List<Review> { new Review { Id = reviewId, SubjectId = coachId } };
            _mockReviewRepository.Setup(r => r.GetReviewsByCoachIdAsync(coachId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(reviews);
            _mockReviewRepository.Setup(r => r.CountReviewsByCoachIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var query = new GetSelfReviewsQuery(coachId, 1, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(reviewId, result.Data.First().Id);
        }

        [Fact]
        public async Task Handle_NoSelfReviews_ReturnsEmptyList()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var page = 1;
            var limit = 10;
            _mockReviewRepository.Setup(r => r.GetReviewsByCoachIdAsync(coachId, page, limit, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Review>());
            _mockReviewRepository.Setup(r => r.CountReviewsByCoachIdAsync(coachId, It.IsAny<CancellationToken>())).ReturnsAsync(0);
            var query = new GetSelfReviewsQuery(coachId, page, limit);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.Count);
            Assert.Equal(page, result.PageIndex);
            Assert.Equal(limit, result.PageSize);
        }
    }
}

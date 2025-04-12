using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Reviews.API.Data;
using Reviews.API.Data.Models;
using Reviews.API.Features.GetFlaggedReviews;
using System.Linq;

namespace Reviews.Test.Features
{
    public class GetFlaggedReviewsHandlerTests
    {
        private readonly DbContextOptions<ReviewDbContext> _options;
        private readonly ReviewDbContext _dbContext;
        private readonly GetFlaggedReviewsHandler _handler;

        public GetFlaggedReviewsHandlerTests()
        {
            // Create in-memory database for testing
            _options = new DbContextOptionsBuilder<ReviewDbContext>()
                .UseInMemoryDatabase(databaseName: $"ReviewsTestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new ReviewDbContext(_options);
            _handler = new GetFlaggedReviewsHandler(_dbContext);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectNumberOfFlaggedReviews()
        {
            // Arrange
            await SeedDatabase();
            var query = new GetFlaggedReviewsQuery(1, 10, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public async Task Handle_WithStatusFilter_ReturnsCorrectFlaggedReviews()
        {
            // Arrange
            await SeedDatabase();
            var query = new GetFlaggedReviewsQuery(1, 10, "Pending");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result.Count);
            Assert.All(result.Data, item => Assert.Equal("Pending", item.Status));
        }

        [Fact]
        public async Task Handle_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await SeedDatabase();
            var query = new GetFlaggedReviewsQuery(2, 1, null); // Page 2, 1 item per page

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(3, result.Count);
            Assert.Equal(2, result.PageIndex);
            Assert.Equal(1, result.PageSize);
        }

        [Fact]
        public async Task Handle_WithNonExistentReview_HandlesDeletedReviewsGracefully()
        {
            // Arrange
            await SeedDatabaseWithNonExistentReview();
            var query = new GetFlaggedReviewsQuery(1, 10, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.Data.Count());
            var firstItem = result.Data.First();
            Assert.Equal(Guid.Empty, firstItem.Review.Id);
            Assert.Equal("Unknown", firstItem.Review.SubjectType);
            Assert.Equal("Review not found or deleted", firstItem.Review.Comment);
        }

        [Fact]
        public async Task Handle_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange - using a clean database
            var query = new GetFlaggedReviewsQuery(1, 10, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.Count);
        }

        private async Task SeedDatabase()
        {
            // Clear the database
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();

            // Create test reviews
            var review1 = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                SubjectType = "Coach",
                SubjectId = Guid.NewGuid(),
                Rating = 4,
                Comment = "Good coach",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            var review2 = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                SubjectType = "Coach",
                SubjectId = Guid.NewGuid(),
                Rating = 2,
                Comment = "Bad experience",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            await _dbContext.Reviews.AddRangeAsync(review1, review2);

            // Create test flags
            var flag1 = new ReviewFlag
            {
                Id = Guid.NewGuid(),
                ReviewId = review1.Id,
                ReportedBy = Guid.NewGuid(),
                FlagReason = "Inappropriate content",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            };

            var flag2 = new ReviewFlag
            {
                Id = Guid.NewGuid(),
                ReviewId = review1.Id,
                ReportedBy = Guid.NewGuid(),
                FlagReason = "Misleading information",
                Status = "Resolved",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var flag3 = new ReviewFlag
            {
                Id = Guid.NewGuid(),
                ReviewId = review2.Id,
                ReportedBy = Guid.NewGuid(),
                FlagReason = "Offensive language",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            await _dbContext.ReviewFlags.AddRangeAsync(flag1, flag2, flag3);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedDatabaseWithNonExistentReview()
        {
            // Clear the database
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();

            // Only create a flag with a non-existent review
            var flag = new ReviewFlag
            {
                Id = Guid.NewGuid(),
                ReviewId = Guid.NewGuid(), // This review doesn't exist
                ReportedBy = Guid.NewGuid(),
                FlagReason = "Inappropriate content",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.ReviewFlags.AddAsync(flag);
            await _dbContext.SaveChangesAsync();
        }
    }
}
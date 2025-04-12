using Microsoft.EntityFrameworkCore;
using Reviews.API.Data;
using Reviews.API.Data.Models;
using Reviews.API.Features.GetReviewStats;

namespace Reviews.Test.Features
{
    public class GetReviewStatsHandlerTests
    {
        private readonly DbContextOptions<ReviewDbContext> _options;
        private readonly ReviewDbContext _dbContext;
        private readonly GetReviewStatsHandler _handler;

        public GetReviewStatsHandlerTests()
        {
            // Create in-memory database for testing
            _options = new DbContextOptionsBuilder<ReviewDbContext>()
                .UseInMemoryDatabase(databaseName: $"ReviewsTestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new ReviewDbContext(_options);
            _handler = new GetReviewStatsHandler(_dbContext);
        }

        [Fact]
        public async Task Handle_NoFilters_ReturnsAllReviews()
        {
            // Arrange
            await SeedDatabase();
            var query = new GetReviewStatsQuery(null, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.TotalReviews);
            Assert.Equal(2, result.ReportedReviews);
            Assert.Null(result.DateRange.StartDate);
            Assert.Null(result.DateRange.EndDate);
        }

        [Fact]
        public async Task Handle_WithStartDateFilter_FiltersReviewsCorrectly()
        {
            // Arrange
            await SeedDatabase();
            var startDate = DateTime.UtcNow.AddDays(-1);
            var query = new GetReviewStatsQuery(startDate, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.TotalReviews); // Only 1 review (the recent one) is after the startDate
            Assert.Equal(1, result.ReportedReviews); // Only 1 reported review after the start date
            Assert.Equal(startDate.ToString("yyyy-MM-dd"), result.DateRange.StartDate);
            Assert.Null(result.DateRange.EndDate);
        }

        [Fact]
        public async Task Handle_WithEndDateFilter_FiltersReviewsCorrectly()
        {
            // Arrange
            await SeedDatabase();
            var endDate = DateTime.UtcNow.AddDays(-2);
            var query = new GetReviewStatsQuery(null, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalReviews); // The oldest and middle reviews are before or on the end date
            Assert.Equal(1, result.ReportedReviews); // Only 1 reported review before or on the end date
            Assert.Null(result.DateRange.StartDate);
            Assert.Equal(endDate.ToString("yyyy-MM-dd"), result.DateRange.EndDate);
        }

        [Fact]
        public async Task Handle_WithDateRangeFilter_FiltersReviewsCorrectly()
        {
            // Arrange
            await SeedDatabase();
            var startDate = DateTime.UtcNow.AddDays(-2);
            var endDate = DateTime.UtcNow;
            var query = new GetReviewStatsQuery(startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.TotalReviews); // Only 1 review (the middle one) is within the date range
            Assert.Equal(1, result.ReportedReviews); // The reported review for the recent review is within the range
            Assert.Equal(startDate.ToString("yyyy-MM-dd"), result.DateRange.StartDate);
            Assert.Equal(endDate.ToString("yyyy-MM-dd"), result.DateRange.EndDate);
        }

        [Fact]
        public async Task Handle_EmptyDatabase_ReturnsZeroCounts()
        {
            // Arrange - using a clean database
            var query = new GetReviewStatsQuery(null, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(0, result.TotalReviews);
            Assert.Equal(0, result.ReportedReviews);
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
                CreatedAt = DateTime.UtcNow.AddDays(-3) // Oldest
            };

            var review2 = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                SubjectType = "Coach",
                SubjectId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Great experience",
                CreatedAt = DateTime.UtcNow.AddDays(-2) // Middle
            };

            var review3 = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                SubjectType = "Coach",
                SubjectId = Guid.NewGuid(),
                Rating = 3,
                Comment = "Average experience",
                CreatedAt = DateTime.UtcNow // Recent
            };

            await _dbContext.Reviews.AddRangeAsync(review1, review2, review3);

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
                ReviewId = review3.Id,
                ReportedBy = Guid.NewGuid(),
                FlagReason = "Misleading information",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.ReviewFlags.AddRangeAsync(flag1, flag2);
            await _dbContext.SaveChangesAsync();
        }
    }
}
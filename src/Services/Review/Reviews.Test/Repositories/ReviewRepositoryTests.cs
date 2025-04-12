using Reviews.API.Data.Models;
using Reviews.Test.Helper;

namespace Reviews.Test.Repositories
{
    public class ReviewRepositoryTests : HandlerTestBase
    {
        [Fact]
        public async Task AddReviewAsync_AddsReviewToDatabase()
        {
            var review = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                SubjectType = "coach",
                SubjectId = Guid.NewGuid(),
                Rating = 4,
            };
            var cancellationToken = CancellationToken.None;

            await Repository.AddReviewAsync(review, cancellationToken);
            await Repository.SaveChangesAsync(cancellationToken);

            Context.Reviews.Should().ContainSingle(r => r.Id == review.Id);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsNonNull_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var review = new Review
            {
                Id = reviewId,
                ReviewerId = Guid.NewGuid(),
                SubjectType = "coach",
                SubjectId = Guid.NewGuid(),
                Rating = 3,
            };
            Context.Reviews.Add(review);
            await Context.SaveChangesAsync(CancellationToken.None);

            var result = await Repository.GetReviewByIdAsync(reviewId, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectId_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var review = new Review
            {
                Id = reviewId,
                ReviewerId = Guid.NewGuid(),
                SubjectType = "coach",
                SubjectId = Guid.NewGuid(),
                Rating = 3,
            };
            Context.Reviews.Add(review);
            await Context.SaveChangesAsync(CancellationToken.None);

            var result = await Repository.GetReviewByIdAsync(reviewId, CancellationToken.None);

            result.Id.Should().Be(reviewId);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectReviewerId_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();
            var review = new Review
            {
                Id = reviewId,
                ReviewerId = reviewerId,
                SubjectType = "coach",
                SubjectId = Guid.NewGuid(),
                Rating = 3,
            };
            Context.Reviews.Add(review);
            await Context.SaveChangesAsync();

            var result = await Repository.GetReviewByIdAsync(reviewId, CancellationToken.None);

            result.ReviewerId.Should().Be(reviewerId);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectSubjectType_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var review = new Review
            {
                Id = reviewId,
                ReviewerId = Guid.NewGuid(),
                SubjectType = "coach",
                SubjectId = Guid.NewGuid(),
                Rating = 3,
            };
            Context.Reviews.Add(review);
            await Context.SaveChangesAsync(CancellationToken.None);

            var result = await Repository.GetReviewByIdAsync(reviewId, CancellationToken.None);

            result.SubjectType.Should().Be("coach");
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectSubjectId_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            var review = new Review
            {
                Id = reviewId,
                ReviewerId = Guid.NewGuid(),
                SubjectType = "coach",
                SubjectId = subjectId,
                Rating = 3,
            };
            Context.Reviews.Add(review);
            await Context.SaveChangesAsync(CancellationToken.None);

            var result = await Repository.GetReviewByIdAsync(reviewId, CancellationToken.None);

            result.SubjectId.Should().Be(subjectId);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectRating_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var review = new Review
            {
                Id = reviewId,
                ReviewerId = Guid.NewGuid(),
                SubjectType = "coach",
                SubjectId = Guid.NewGuid(),
                Rating = 4,
            };
            Context.Reviews.Add(review);
            await Context.SaveChangesAsync(CancellationToken.None);

            var result = await Repository.GetReviewByIdAsync(reviewId, CancellationToken.None);

            result.Rating.Should().Be(4);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsNull_WhenReviewDoesNotExist()
        {
            var reviewId = Guid.NewGuid();

            var result = await Repository.GetReviewByIdAsync(reviewId, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task RemoveReviewAsync_RemovesReviewFromDatabase()
        {
            var review = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                SubjectType = "coach",
                SubjectId = Guid.NewGuid(),
                Rating = 3,
            };
            Context.Reviews.Add(review);
            await Context.SaveChangesAsync();

            await Repository.RemoveReviewAsync(review, CancellationToken.None);
            await Repository.SaveChangesAsync(CancellationToken.None);

            Context.Reviews.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReviewsBySubjectAsync_ReturnsNonNullList()
        {
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            Context.Reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                SubjectType = subjectType,
                SubjectId = subjectId,
                Rating = 3,
            });
            await Context.SaveChangesAsync(CancellationToken.None);

            var result = await Repository.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 10, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetReviewsBySubjectAsync_ReturnsCorrectCount()
        {
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            Context.Reviews.AddRange(
                new Review
                {
                    Id = Guid.NewGuid(),
                    ReviewerId = Guid.NewGuid(),
                    SubjectType = subjectType,
                    SubjectId = subjectId,
                    Rating = 3,
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    ReviewerId = Guid.NewGuid(),
                    SubjectType = subjectType,
                    SubjectId = subjectId,
                    Rating = 4,
                }
            );
            await Context.SaveChangesAsync(CancellationToken.None);

            var result = await Repository.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 10, CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetReviewRepliesAsync_ReturnsNonNullList()
        {
            var reviewId = Guid.NewGuid();
            Context.ReviewReplies.Add(new ReviewReply
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                ResponderId = Guid.NewGuid(),
                ReplyText = "Test reply",
            });
            await Context.SaveChangesAsync(CancellationToken.None);

            var result = await Repository.GetReviewRepliesAsync(reviewId, 1, 5, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetReviewsByCoachIdAsync_ReturnsNonNullList()
        {
            var coachId = Guid.NewGuid();
            Context.Reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = Guid.NewGuid(),
                SubjectType = "coach",
                SubjectId = coachId,
                Rating = 3,
            });
            await Context.SaveChangesAsync(CancellationToken.None);

            var result = await Repository.GetReviewsByCoachIdAsync(coachId, 1, 10, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task AddReviewFlagAsync_AddsFlagToDatabase()
        {
            // Arrange
            var flag = new ReviewFlag
            {
                Id = Guid.NewGuid(),
                ReviewId = Guid.NewGuid(),
                ReportedBy = Guid.NewGuid(),
                FlagReason = "Inappropriate content",
                Status = "Pending"
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await Repository.AddReviewFlagAsync(flag, cancellationToken);
            await Repository.SaveChangesAsync(cancellationToken);

            // Assert
            var savedFlag = await Context.ReviewFlags.FindAsync(flag.Id);
            savedFlag.Should().NotBeNull();
            savedFlag.FlagReason.Should().Be("Inappropriate content");
            savedFlag.Status.Should().Be("Pending");
        }

        [Fact]
        public async Task GetReviewFlagByIdAsync_ReturnsFlagWhenExists()
        {
            // Arrange
            var flagId = Guid.NewGuid();
            var flag = new ReviewFlag
            {
                Id = flagId,
                ReviewId = Guid.NewGuid(),
                ReportedBy = Guid.NewGuid(),
                FlagReason = "Offensive language",
                Status = "Pending"
            };
            Context.ReviewFlags.Add(flag);
            await Context.SaveChangesAsync();

            // Act
            var result = await Repository.GetReviewFlagByIdAsync(flagId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(flagId);
        }

        [Fact]
        public async Task GetReviewFlagByIdAsync_ReturnsNullWhenNotExists()
        {
            // Arrange
            var flagId = Guid.NewGuid();

            // Act
            var result = await Repository.GetReviewFlagByIdAsync(flagId, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateReviewFlagAsync_UpdatesFlagInDatabase()
        {
            // Arrange
            var flagId = Guid.NewGuid();
            var flag = new ReviewFlag
            {
                Id = flagId,
                ReviewId = Guid.NewGuid(),
                ReportedBy = Guid.NewGuid(),
                FlagReason = "Inappropriate content",
                Status = "Pending"
            };
            Context.ReviewFlags.Add(flag);
            await Context.SaveChangesAsync();

            // Update the flag
            flag.Status = "Resolved";

            // Act
            await Repository.UpdateReviewFlagAsync(flag, CancellationToken.None);
            await Repository.SaveChangesAsync(CancellationToken.None);

            // Assert
            var updatedFlag = await Context.ReviewFlags.FindAsync(flagId);
            updatedFlag.Status.Should().Be("Resolved");
        }

        [Fact]
        public async Task AddReviewReplyAsync_AddsReplyToDatabase()
        {
            // Arrange
            var reply = new ReviewReply
            {
                Id = Guid.NewGuid(),
                ReviewId = Guid.NewGuid(),
                ResponderId = Guid.NewGuid(),
                ReplyText = "This is a test reply"
            };

            // Act
            await Repository.AddReviewReplyAsync(reply, CancellationToken.None);
            await Repository.SaveChangesAsync(CancellationToken.None);

            // Assert
            var savedReply = await Context.ReviewReplies.FindAsync(reply.Id);
            savedReply.Should().NotBeNull();
            savedReply.ReplyText.Should().Be("This is a test reply");
        }

        [Fact]
        public async Task CountReviewsBySubjectAsync_ReturnsCorrectCount()
        {
            // Arrange
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            Context.Reviews.AddRange(
                new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId },
                new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId },
                new Review { Id = Guid.NewGuid(), SubjectType = subjectType, SubjectId = subjectId }
            );
            await Context.SaveChangesAsync();

            // Act
            var result = await Repository.CountReviewsBySubjectAsync(subjectType, subjectId, CancellationToken.None);

            // Assert
            result.Should().Be(3);
        }
    }
}
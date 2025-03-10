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
            var flag = new ReviewFlag
            {
                Id = Guid.NewGuid(),
                ReviewId = Guid.NewGuid(),
                ReportedBy = Guid.NewGuid(),
                FlagReason = "Inappropriate content",
                Status = "Pending"
            };
            var cancellationToken = CancellationToken.None;

            await Repository.AddReviewFlagAsync(flag, cancellationToken);
            await Repository.SaveChangesAsync(cancellationToken);

            Context.ReviewFlags.Should().ContainSingle(f => f.Id == flag.Id);
        }

        [Fact]
        public async Task AddReviewReplyAsync_AddsReplyToDatabase()
        {
            var reply = new ReviewReply
            {
                Id = Guid.NewGuid(),
                ReviewId = Guid.NewGuid(),
                ResponderId = Guid.NewGuid(),
                ReplyText = "Test reply",
            };
            var cancellationToken = CancellationToken.None;

            await Repository.AddReviewReplyAsync(reply, cancellationToken);
            await Repository.SaveChangesAsync(cancellationToken);

            Context.ReviewReplies.Should().ContainSingle(r => r.Id == reply.Id);
        }
    }
}
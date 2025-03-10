using Microsoft.EntityFrameworkCore;
using Reviews.API.Data;
using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;

namespace Reviews.Test.Repositories
{
    public class ReviewRepositoryTests
    {
        private readonly Mock<IReviewDbContext> _mockContext;
        private readonly Mock<DbSet<Review>> _mockReviewSet;
        private readonly Mock<DbSet<ReviewFlag>> _mockReviewFlagSet;
        private readonly Mock<DbSet<ReviewReply>> _mockReviewReplySet;
        private readonly ReviewRepository _repository;

        public ReviewRepositoryTests()
        {
            _mockContext = new Mock<IReviewDbContext>();
            _mockReviewSet = new Mock<DbSet<Review>>();
            _mockReviewFlagSet = new Mock<DbSet<ReviewFlag>>();
            _mockReviewReplySet = new Mock<DbSet<ReviewReply>>();

            // Set up interfaces before accessing .Object
            _mockReviewSet.As<IQueryable<Review>>();
            _mockReviewSet.As<IAsyncEnumerable<Review>>();
            _mockReviewReplySet.As<IQueryable<ReviewReply>>();
            _mockReviewReplySet.As<IAsyncEnumerable<ReviewReply>>();

            _mockContext.Setup(m => m.Reviews).Returns(_mockReviewSet.Object);
            _mockContext.Setup(m => m.ReviewFlags).Returns(_mockReviewFlagSet.Object);
            _mockContext.Setup(m => m.ReviewReplies).Returns(_mockReviewReplySet.Object);

            _repository = new ReviewRepository(_mockContext.Object);
        }

        [Fact]
        public async Task AddReviewAsync_CallsAddAsyncOnReviewSet()
        {
            var review = new Review { Id = Guid.NewGuid() };
            var cancellationToken = CancellationToken.None;

            await _repository.AddReviewAsync(review, cancellationToken);

            _mockReviewSet.Verify(m => m.AddAsync(review, cancellationToken), Times.Once());
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsNonNull_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var expectedReview = new Review { Id = reviewId };
            var reviews = new List<Review> { expectedReview }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewByIdAsync(reviewId, cancellationToken);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectId_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var expectedReview = new Review { Id = reviewId };
            var reviews = new List<Review> { expectedReview }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewByIdAsync(reviewId, cancellationToken);

            Assert.Equal(expectedReview.Id, result!.Id);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectReviewerId_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();
            var expectedReview = new Review { Id = reviewId, ReviewerId = reviewerId };
            var reviews = new List<Review> { expectedReview }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewByIdAsync(reviewId, cancellationToken);

            Assert.Equal(expectedReview.ReviewerId, result!.ReviewerId);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectSubjectType_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var expectedReview = new Review { Id = reviewId, SubjectType = "coach" };
            var reviews = new List<Review> { expectedReview }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewByIdAsync(reviewId, cancellationToken);

            Assert.Equal(expectedReview.SubjectType, result!.SubjectType);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectSubjectId_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            var expectedReview = new Review { Id = reviewId, SubjectId = subjectId };
            var reviews = new List<Review> { expectedReview }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewByIdAsync(reviewId, cancellationToken);

            Assert.Equal(expectedReview.SubjectId, result!.SubjectId);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsCorrectRating_WhenReviewExists()
        {
            var reviewId = Guid.NewGuid();
            var expectedReview = new Review { Id = reviewId, Rating = 4 };
            var reviews = new List<Review> { expectedReview }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewByIdAsync(reviewId, cancellationToken);

            Assert.Equal(expectedReview.Rating, result!.Rating);
        }

        [Fact]
        public async Task GetReviewByIdAsync_ReturnsNull_WhenReviewDoesNotExist()
        {
            var reviewId = Guid.NewGuid();
            var reviews = new List<Review>().AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewByIdAsync(reviewId, cancellationToken);

            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveReviewAsync_CallsRemoveOnReviewSet()
        {
            var review = new Review { Id = Guid.NewGuid() };
            var cancellationToken = CancellationToken.None;

            await _repository.RemoveReviewAsync(review, cancellationToken);

            _mockReviewSet.Verify(m => m.Remove(review), Times.Once());
        }

        [Fact]
        public async Task SaveChangesAsync_CallsSaveChangesAsyncOnContext()
        {
            var cancellationToken = CancellationToken.None;

            await _repository.SaveChangesAsync(cancellationToken);

            _mockContext.Verify(m => m.SaveChangesAsync(cancellationToken), Times.Once());
        }

        [Fact]
        public async Task GetReviewsBySubjectAsync_ReturnsNonNullList()
        {
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            var reviews = new List<Review> { new Review { SubjectType = subjectType, SubjectId = subjectId, CreatedAt = DateTime.UtcNow } }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 10, cancellationToken);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetReviewsBySubjectAsync_ReturnsCorrectCount()
        {
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { SubjectType = subjectType, SubjectId = subjectId, CreatedAt = DateTime.UtcNow },
                new Review { SubjectType = subjectType, SubjectId = subjectId, CreatedAt = DateTime.UtcNow }
            }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 10, cancellationToken);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetReviewsBySubjectAsync_ReturnsReviewsWithCorrectSubjectType()
        {
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            var reviews = new List<Review> { new Review { SubjectType = subjectType, SubjectId = subjectId, CreatedAt = DateTime.UtcNow } }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 10, cancellationToken);

            Assert.Equal(subjectType, result[0].SubjectType);
        }

        [Fact]
        public async Task GetReviewsBySubjectAsync_ReturnsReviewsWithCorrectSubjectId()
        {
            var subjectType = "court";
            var subjectId = Guid.NewGuid();
            var reviews = new List<Review> { new Review { SubjectType = subjectType, SubjectId = subjectId, CreatedAt = DateTime.UtcNow } }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewsBySubjectAsync(subjectType, subjectId, 1, 10, cancellationToken);

            Assert.Equal(subjectId, result[0].SubjectId);
        }

        [Fact]
        public async Task GetReviewRepliesAsync_ReturnsNonNullList()
        {
            var reviewId = Guid.NewGuid();
            var replies = new List<ReviewReply> { new ReviewReply { ReviewId = reviewId, CreatedAt = DateTime.UtcNow } }.AsQueryable();
            SetupReplyQueryable(replies);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewRepliesAsync(reviewId, 1, 5, cancellationToken);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetReviewRepliesAsync_ReturnsCorrectCount()
        {
            var reviewId = Guid.NewGuid();
            var replies = new List<ReviewReply>
            {
                new ReviewReply { ReviewId = reviewId, CreatedAt = DateTime.UtcNow },
                new ReviewReply { ReviewId = reviewId, CreatedAt = DateTime.UtcNow }
            }.AsQueryable();
            SetupReplyQueryable(replies);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewRepliesAsync(reviewId, 1, 5, cancellationToken);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetReviewRepliesAsync_ReturnsRepliesWithCorrectReviewId()
        {
            var reviewId = Guid.NewGuid();
            var replies = new List<ReviewReply> { new ReviewReply { ReviewId = reviewId, CreatedAt = DateTime.UtcNow } }.AsQueryable();
            SetupReplyQueryable(replies);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewRepliesAsync(reviewId, 1, 5, cancellationToken);

            Assert.Equal(reviewId, result[0].ReviewId);
        }

        [Fact]
        public async Task GetReviewsByCoachIdAsync_ReturnsNonNullList()
        {
            var coachId = Guid.NewGuid();
            var reviews = new List<Review> { new Review { SubjectType = "coach", SubjectId = coachId, CreatedAt = DateTime.UtcNow } }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewsByCoachIdAsync(coachId, 1, 10, cancellationToken);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetReviewsByCoachIdAsync_ReturnsCorrectCount()
        {
            var coachId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { SubjectType = "coach", SubjectId = coachId, CreatedAt = DateTime.UtcNow },
                new Review { SubjectType = "coach", SubjectId = coachId, CreatedAt = DateTime.UtcNow }
            }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewsByCoachIdAsync(coachId, 1, 10, cancellationToken);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetReviewsByCoachIdAsync_ReturnsReviewsWithCorrectSubjectType()
        {
            var coachId = Guid.NewGuid();
            var reviews = new List<Review> { new Review { SubjectType = "coach", SubjectId = coachId, CreatedAt = DateTime.UtcNow } }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewsByCoachIdAsync(coachId, 1, 10, cancellationToken);

            Assert.Equal("coach", result[0].SubjectType);
        }

        [Fact]
        public async Task GetReviewsByCoachIdAsync_ReturnsReviewsWithCorrectSubjectId()
        {
            var coachId = Guid.NewGuid();
            var reviews = new List<Review> { new Review { SubjectType = "coach", SubjectId = coachId, CreatedAt = DateTime.UtcNow } }.AsQueryable();
            SetupReviewQueryable(reviews);
            var cancellationToken = CancellationToken.None;

            var result = await _repository.GetReviewsByCoachIdAsync(coachId, 1, 10, cancellationToken);

            Assert.Equal(coachId, result[0].SubjectId);
        }

        [Fact]
        public async Task AddReviewFlagAsync_CallsAddAsyncOnFlagSet()
        {
            var flag = new ReviewFlag { Id = Guid.NewGuid() };
            var cancellationToken = CancellationToken.None;

            await _repository.AddReviewFlagAsync(flag, cancellationToken);

            _mockReviewFlagSet.Verify(m => m.AddAsync(flag, cancellationToken), Times.Once());
        }

        [Fact]
        public async Task AddReviewReplyAsync_CallsAddAsyncOnReplySet()
        {
            var reply = new ReviewReply { Id = Guid.NewGuid() };
            var cancellationToken = CancellationToken.None;

            await _repository.AddReviewReplyAsync(reply, cancellationToken);

            _mockReviewReplySet.Verify(m => m.AddAsync(reply, cancellationToken), Times.Once());
        }

        private void SetupReviewQueryable(IQueryable<Review> reviews)
        {
            _mockReviewSet.As<IQueryable<Review>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Review>(reviews.Provider));
            _mockReviewSet.As<IQueryable<Review>>().Setup(m => m.Expression).Returns(reviews.Expression);
            _mockReviewSet.As<IQueryable<Review>>().Setup(m => m.ElementType).Returns(reviews.ElementType);
            _mockReviewSet.As<IQueryable<Review>>().Setup(m => m.GetEnumerator()).Returns(reviews.GetEnumerator());

            _mockReviewSet.As<IAsyncEnumerable<Review>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Review>(reviews.GetEnumerator()));
        }

        private void SetupReplyQueryable(IQueryable<ReviewReply> replies)
        {
            _mockReviewReplySet.As<IQueryable<ReviewReply>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ReviewReply>(replies.Provider));
            _mockReviewReplySet.As<IQueryable<ReviewReply>>().Setup(m => m.Expression).Returns(replies.Expression);
            _mockReviewReplySet.As<IQueryable<ReviewReply>>().Setup(m => m.ElementType).Returns(replies.ElementType);
            _mockReviewReplySet.As<IQueryable<ReviewReply>>().Setup(m => m.GetEnumerator()).Returns(replies.GetEnumerator());

            _mockReviewReplySet.As<IAsyncEnumerable<ReviewReply>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ReviewReply>(replies.GetEnumerator()));
        }
    }
}
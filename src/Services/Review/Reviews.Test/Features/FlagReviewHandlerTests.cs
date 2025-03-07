using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.FlagReview;

namespace Reviews.Test.Features
{
    public class FlagReviewHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly FlagReviewHandler _handler;

        public FlagReviewHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _handler = new FlagReviewHandler(_mockReviewRepository.Object);
        }
        [Fact]
        public async Task Handle_ReturnsValidFlagId()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reportedBy = Guid.NewGuid();
            var flagReason = "Inappropriate content";
            var command = new FlagReviewCommand(reviewId, reportedBy, flagReason);

            // Act
            var flagId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, flagId);
        }

        [Fact]
        public async Task Handle_AddsFlagWithCorrectReviewId()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reportedBy = Guid.NewGuid();
            var flagReason = "Inappropriate content";
            var command = new FlagReviewCommand(reviewId, reportedBy, flagReason);
            ReviewFlag addedFlag = null;
            _mockReviewRepository.Setup(r => r.AddReviewFlagAsync(It.IsAny<ReviewFlag>(), It.IsAny<CancellationToken>()))
                .Callback<ReviewFlag, CancellationToken>((flag, ct) => addedFlag = flag);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(reviewId, addedFlag.ReviewId);
        }

        [Fact]
        public async Task Handle_AddsFlagWithCorrectReportedBy()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reportedBy = Guid.NewGuid();
            var flagReason = "Inappropriate content";
            var command = new FlagReviewCommand(reviewId, reportedBy, flagReason);
            ReviewFlag addedFlag = null;
            _mockReviewRepository.Setup(r => r.AddReviewFlagAsync(It.IsAny<ReviewFlag>(), It.IsAny<CancellationToken>()))
                .Callback<ReviewFlag, CancellationToken>((flag, ct) => addedFlag = flag);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(reportedBy, addedFlag.ReportedBy);
        }

        [Fact]
        public async Task Handle_AddsFlagWithCorrectFlagReason()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reportedBy = Guid.NewGuid();
            var flagReason = "Inappropriate content";
            var command = new FlagReviewCommand(reviewId, reportedBy, flagReason);
            ReviewFlag addedFlag = null;
            _mockReviewRepository.Setup(r => r.AddReviewFlagAsync(It.IsAny<ReviewFlag>(), It.IsAny<CancellationToken>()))
                .Callback<ReviewFlag, CancellationToken>((flag, ct) => addedFlag = flag);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(flagReason, addedFlag.FlagReason);
        }

        [Fact]
        public async Task Handle_AddsFlagWithPendingStatus()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reportedBy = Guid.NewGuid();
            var flagReason = "Inappropriate content";
            var command = new FlagReviewCommand(reviewId, reportedBy, flagReason);
            ReviewFlag addedFlag = null;
            _mockReviewRepository.Setup(r => r.AddReviewFlagAsync(It.IsAny<ReviewFlag>(), It.IsAny<CancellationToken>()))
                .Callback<ReviewFlag, CancellationToken>((flag, ct) => addedFlag = flag);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("pending", addedFlag.Status);
        }

        [Fact]
        public async Task Handle_AddsFlagWithValidCreatedAt()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reportedBy = Guid.NewGuid();
            var flagReason = "Inappropriate content";
            var command = new FlagReviewCommand(reviewId, reportedBy, flagReason);
            ReviewFlag addedFlag = null;
            _mockReviewRepository.Setup(r => r.AddReviewFlagAsync(It.IsAny<ReviewFlag>(), It.IsAny<CancellationToken>()))
                .Callback<ReviewFlag, CancellationToken>((flag, ct) => addedFlag = flag);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(addedFlag.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public async Task Handle_AddsFlagWithValidUpdatedAt()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reportedBy = Guid.NewGuid();
            var flagReason = "Inappropriate content";
            var command = new FlagReviewCommand(reviewId, reportedBy, flagReason);
            ReviewFlag addedFlag = null;
            _mockReviewRepository.Setup(r => r.AddReviewFlagAsync(It.IsAny<ReviewFlag>(), It.IsAny<CancellationToken>()))
                .Callback<ReviewFlag, CancellationToken>((flag, ct) => addedFlag = flag);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(addedFlag.UpdatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public async Task Handle_CallsAddReviewFlagAsync()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reportedBy = Guid.NewGuid();
            var flagReason = "Inappropriate content";
            var command = new FlagReviewCommand(reviewId, reportedBy, flagReason);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockReviewRepository.Verify(r => r.AddReviewFlagAsync(It.IsAny<ReviewFlag>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CallsSaveChangesAsync()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var reportedBy = Guid.NewGuid();
            var flagReason = "Inappropriate content";
            var command = new FlagReviewCommand(reviewId, reportedBy, flagReason);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

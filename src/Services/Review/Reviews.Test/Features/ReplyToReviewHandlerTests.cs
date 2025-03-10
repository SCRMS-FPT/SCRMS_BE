using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.ReplyToReview;

namespace Reviews.Test.Features
{
    public class ReplyToReviewHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly ReplyToReviewHandler _handler;

        public ReplyToReviewHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _handler = new ReplyToReviewHandler(_mockReviewRepository.Object);
        }
        [Fact]
        public async Task Handle_AddsReply_ReturnsValidReplyId()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var command = new ReplyToReviewCommand(reviewId, responderId, "Thanks");

            // Act
            var replyId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, replyId);
        }

        [Fact]
        public async Task Handle_AddsReply_SetsCorrectReviewId()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var command = new ReplyToReviewCommand(reviewId, responderId, "Thanks");
            ReviewReply addedReply = null;
            _mockReviewRepository.Setup(r => r.AddReviewReplyAsync(It.IsAny<ReviewReply>(), It.IsAny<CancellationToken>()))
                .Callback<ReviewReply, CancellationToken>((reply, ct) => addedReply = reply);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(reviewId, addedReply.ReviewId);
        }

        [Fact]
        public async Task Handle_AddsReply_SetsCorrectResponderId()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var command = new ReplyToReviewCommand(reviewId, responderId, "Thanks");
            ReviewReply addedReply = null;
            _mockReviewRepository.Setup(r => r.AddReviewReplyAsync(It.IsAny<ReviewReply>(), It.IsAny<CancellationToken>()))
                .Callback<ReviewReply, CancellationToken>((reply, ct) => addedReply = reply);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(responderId, addedReply.ResponderId);
        }

        [Fact]
        public async Task Handle_AddsReply_SetsCorrectReplyText()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var replyText = "Thank you!";
            var command = new ReplyToReviewCommand(reviewId, responderId, replyText);
            ReviewReply addedReply = null;
            _mockReviewRepository.Setup(r => r.AddReviewReplyAsync(It.IsAny<ReviewReply>(), It.IsAny<CancellationToken>()))
                .Callback<ReviewReply, CancellationToken>((reply, ct) => addedReply = reply);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(replyText, addedReply.ReplyText);
        }

        [Fact]
        public async Task Handle_AddsReply_CallsAddReviewReplyAsync()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var command = new ReplyToReviewCommand(reviewId, responderId, "Thanks");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockReviewRepository.Verify(r => r.AddReviewReplyAsync(It.IsAny<ReviewReply>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AddsReply_CallsSaveChangesAsync()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var command = new ReplyToReviewCommand(reviewId, responderId, "Thanks");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

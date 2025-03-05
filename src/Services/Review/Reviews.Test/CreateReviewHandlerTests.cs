using Microsoft.Extensions.Logging;
using Reviews.API.Cache;
using Reviews.API.Clients;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.CreateReview;
using Xunit;
namespace Review.Test
{
    public class CreateReviewHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepo;
        private readonly Mock<ISubjectCache> _mockCache;
        private readonly Mock<ICoachServiceClient> _mockCoachClient;
        private readonly Mock<ICourtServiceClient> _mockCourtClient;
        private readonly Mock<ILogger<CreateReviewHandler>> _mockLogger;
        private readonly CreateReviewHandler _handler;

        public CreateReviewHandlerTests()
        {
            _mockReviewRepo = new Mock<IReviewRepository>();
            _mockCache = new Mock<ISubjectCache>();
            _mockCoachClient = new Mock<ICoachServiceClient>();
            _mockCourtClient = new Mock<ICourtServiceClient>();
            _mockLogger = new Mock<ILogger<CreateReviewHandler>>();

            _handler = new CreateReviewHandler(
                _mockReviewRepo.Object,
                _mockCache.Object,
                _mockCoachClient.Object,
                _mockCourtClient.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldCreateReview_WhenValidRequest()
        {
            // Arrange
            var command = new CreateReviewCommand(
                Guid.NewGuid(),
                "coach",
                Guid.NewGuid(),
                5,
                "Great coach!"
            );

            _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<bool>.IsAny)).Returns(false);
            _mockCoachClient.Setup(c => c.CoachExistsAsync(command.SubjectId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockReviewRepo.Setup(r => r.AddReviewAsync(It.IsAny<Reviews.API.Data.Models.Review>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockReviewRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            _mockReviewRepo.Verify(r => r.AddReviewAsync(It.IsAny<Reviews.API.Data.Models.Review>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRatingOutOfRange()
        {

            var command = new CreateReviewCommand(Guid.NewGuid(), "coach", Guid.NewGuid(), 6, "Too high!");

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Rating must be between 1 and 5.");
        }

    }

}

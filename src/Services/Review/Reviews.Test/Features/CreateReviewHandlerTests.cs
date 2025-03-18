using Microsoft.Extensions.Logging;
using Reviews.API.Cache;
using Reviews.API.Clients;
using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.CreateReview;
namespace Reviews.Test.Features
{
    public class CreateReviewHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockRepository;
        private readonly Mock<ISubjectCache> _mockCache;
        private readonly Mock<ICoachServiceClient> _mockCoachClient;
        private readonly Mock<ICourtServiceClient> _mockCourtClient;
        private readonly Mock<ILogger<CreateReviewHandler>> _mockLogger;
        private readonly CreateReviewHandler _handler;

        public CreateReviewHandlerTests()
        {
            _mockRepository = new Mock<IReviewRepository>();
            _mockCache = new Mock<ISubjectCache>();
            _mockCoachClient = new Mock<ICoachServiceClient>();
            _mockCourtClient = new Mock<ICourtServiceClient>();
            _mockLogger = new Mock<ILogger<CreateReviewHandler>>();

            _handler = new CreateReviewHandler(
                _mockRepository.Object,
                _mockCache.Object,
                _mockCoachClient.Object,
                _mockCourtClient.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_RatingLessThanOne_ThrowsArgumentException()
        {
            // Arrange
            var command = new CreateReviewCommand(Guid.NewGuid(), "court", Guid.NewGuid(), 0, "Comment");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Rating must be between 1 and 5.", exception.Message);
        }
        [Fact]
        public async Task Handle_RatingGreaterThanFive_ThrowsArgumentException()
        {
            // Arrange
            var command = new CreateReviewCommand(Guid.NewGuid(), "coach", Guid.NewGuid(), 6, "Comment");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Rating must be between 1 and 5.", exception.Message);
        }
        [Fact]
        public async Task Handle_InvalidSubjectType_ThrowsArgumentException()
        {
            // Arrange
            var command = new CreateReviewCommand(Guid.NewGuid(), "invalid", Guid.NewGuid(), 3, "Comment");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Invalid subject type.", exception.Message);
        }
        [Fact]
        public async Task Handle_SubjectExistsInCache_CreatesReviewSuccessfully()
        {
            // Arrange
            var subjectId = Guid.NewGuid();
            var command = new CreateReviewCommand(Guid.NewGuid(), "court", subjectId, 4, "Comment");
            bool exists = true;
            _mockCache.Setup(c => c.TryGetValue($"court_{subjectId}", out exists)).Returns(true);
            _mockRepository.Setup(r => r.AddReviewAsync(It.IsAny<Review>(), CancellationToken.None)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.SaveChangesAsync(CancellationToken.None)).Returns(Task.CompletedTask);

            // Act
            var reviewId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.AddReviewAsync(It.Is<Review>(rev =>
                rev.SubjectId == subjectId && rev.Rating == 4), CancellationToken.None), Times.Once());
            _mockRepository.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Once());
            Assert.NotEqual(Guid.Empty, reviewId);
        }
        [Fact]
        public async Task Handle_SubjectNotInCacheExistsViaApi_CreatesReviewSuccessfully()
        {
            // Arrange
            var subjectId = Guid.NewGuid();
            var command = new CreateReviewCommand(Guid.NewGuid(), "coach", subjectId, 3, "Comment");
            bool exists = false;
            _mockCache.Setup(c => c.TryGetValue($"coach_{subjectId}", out exists)).Returns(false);
            _mockCoachClient.Setup(c => c.CoachExistsAsync(subjectId, CancellationToken.None)).ReturnsAsync(true);
            _mockCache.Setup(c => c.Set($"coach_{subjectId}", true, It.IsAny<TimeSpan?>()));
            _mockRepository.Setup(r => r.AddReviewAsync(It.IsAny<Review>(), CancellationToken.None)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.SaveChangesAsync(CancellationToken.None)).Returns(Task.CompletedTask);

            // Act
            var reviewId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockCoachClient.Verify(c => c.CoachExistsAsync(subjectId, CancellationToken.None), Times.Once());
            _mockCache.Verify(c => c.Set($"coach_{subjectId}", true, It.IsAny<TimeSpan?>()), Times.Once());
            _mockRepository.Verify(r => r.AddReviewAsync(It.IsAny<Review>(), CancellationToken.None), Times.Once());
            _mockRepository.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Once());
            Assert.NotEqual(Guid.Empty, reviewId);
        }
        [Fact]
        public async Task Handle_SubjectDoesNotExistViaApi_ThrowsInvalidOperationException()
        {
            // Arrange
            var subjectId = Guid.NewGuid();
            var command = new CreateReviewCommand(Guid.NewGuid(), "court", subjectId, 2, "Comment");
            bool exists = false;
            _mockCache.Setup(c => c.TryGetValue($"court_{subjectId}", out exists)).Returns(false);
            _mockCourtClient.Setup(c => c.CourtExistsAsync(subjectId, CancellationToken.None)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("court not exists", exception.Message);
        }
        [Fact]
        public async Task Handle_NullComment_CreatesReviewSuccessfully()
        {
            // Arrange
            var subjectId = Guid.NewGuid();
            var command = new CreateReviewCommand(Guid.NewGuid(), "court", subjectId, 5, null);
            bool exists = true;
            _mockCache.Setup(c => c.TryGetValue($"court_{subjectId}", out exists)).Returns(true);
            _mockRepository.Setup(r => r.AddReviewAsync(It.IsAny<Review>(), CancellationToken.None)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.SaveChangesAsync(CancellationToken.None)).Returns(Task.CompletedTask);

            // Act
            var reviewId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.AddReviewAsync(It.Is<Review>(rev => rev.Comment == null), CancellationToken.None), Times.Once());
            _mockRepository.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Once());
            Assert.NotEqual(Guid.Empty, reviewId);
        }
    }

}

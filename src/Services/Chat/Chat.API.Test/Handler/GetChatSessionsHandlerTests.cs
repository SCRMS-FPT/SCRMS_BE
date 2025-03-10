using Chat.API.Data.Repositories;
using Chat.API.Features.GetChatSessions;
using Moq;
using Xunit;

namespace Chat.API.Tests.Handler
{
    public class GetChatSessionsHandlerTests
    {
        private readonly Mock<IChatSessionRepository> _repositoryMock;
        private readonly GetChatSessionsHandler _handler;

        public GetChatSessionsHandlerTests()
        {
            _repositoryMock = new Mock<IChatSessionRepository>();
            _handler = new GetChatSessionsHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSessions_WhenUserHasSessions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sessions = new List<ChatSessionResponse> { new ChatSessionResponse(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow) };
            _repositoryMock.Setup(r => r.GetChatSessionByUserIdAsync(userId)).ReturnsAsync(sessions);
            var query = new GetChatSessionsQuery(1, 10, userId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(sessions, result);
            Assert.Single(result);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoSessions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetChatSessionByUserIdAsync(userId)).ReturnsAsync(new List<ChatSessionResponse>());
            var query = new GetChatSessionsQuery(1, 10, userId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenUserIdIsEmpty()
        {
            // Arrange
            var query = new GetChatSessionsQuery(1, 10, Guid.Empty);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
            Assert.Equal("UserId cannot be empty", exception.Message); // Giả định logic xử lý lỗi
        }
    }
}
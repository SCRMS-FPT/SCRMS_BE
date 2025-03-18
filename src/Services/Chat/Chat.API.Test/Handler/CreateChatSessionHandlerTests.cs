using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Chat.API.Features.CreateChatSession;
using Moq;
using Xunit;

namespace Chat.API.Tests.Handler
{
    public class CreateChatSessionHandlerTests
    {
        private readonly Mock<IChatSessionRepository> _repositoryMock;
        private readonly CreateChatSessionHandler _handler;

        public CreateChatSessionHandlerTests()
        {
            _repositoryMock = new Mock<IChatSessionRepository>();
            _handler = new CreateChatSessionHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_CreatesNewSession_WhenNoExistingSession()
        {
            // Arrange
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetChatSessionByUsersAsync(user1Id, user2Id)).ReturnsAsync((ChatSession)null);
            var command = new CreateChatSessionCommand(user1Id, user2Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.ChatSessionId);
            _repositoryMock.Verify(r => r.AddChatSessionAsync(It.Is<ChatSession>(s => s.User1Id == user1Id && s.User2Id == user2Id)), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsExistingSession_WhenSessionExists()
        {
            // Arrange
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var existingSession = new ChatSession { Id = Guid.NewGuid(), User1Id = user1Id, User2Id = user2Id };
            _repositoryMock.Setup(r => r.GetChatSessionByUsersAsync(user1Id, user2Id)).ReturnsAsync(existingSession);
            var command = new CreateChatSessionCommand(user1Id, user2Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(existingSession.Id, result.ChatSessionId);
            _repositoryMock.Verify(r => r.AddChatSessionAsync(It.IsAny<ChatSession>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenUsersAreSame()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateChatSessionCommand(userId, userId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("User1Id and User2Id cannot be the same", exception.Message); // Giả định logic xử lý lỗi trong handler
        }
    }
}
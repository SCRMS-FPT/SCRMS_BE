using Chat.API.Data;
using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Chat.API.Features.EditMessage;
using Moq;
using Xunit;

namespace Chat.API.Tests.Handler
{
    public class EditMessageHandlerTests
    {
        private readonly Mock<IChatMessageRepository> _repositoryMock;
        private readonly EditMessageHandler _handler;

        public EditMessageHandlerTests()
        {
            _repositoryMock = new Mock<IChatMessageRepository>();
            _handler = new EditMessageHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_UpdatesMessage_WhenAuthorized()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var message = new ChatMessage { Id = messageId, ChatSessionId = sessionId, SenderId = userId, MessageText = "Old Text" };
            _repositoryMock.Setup(r => r.GetChatMessageByIdAndSessionAsync(messageId, sessionId, userId)).ReturnsAsync(message);
            var command = new EditMessageCommand(sessionId, messageId, "New Text", userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("New Text", message.MessageText);
            Assert.NotEqual(default(DateTime), message.UpdatedAt);
            _repositoryMock.Verify(r => r.UpdateChatMessageAsync(message), Times.Once);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenMessageNotFoundOrUnauthorized()
        {
            // Arrange
            var command = new EditMessageCommand(Guid.NewGuid(), Guid.NewGuid(), "Text", Guid.NewGuid());
            _repositoryMock.Setup(r => r.GetChatMessageByIdAndSessionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((ChatMessage)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Message not found or not authorized", exception.Message);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenMessageTextIsEmpty()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var message = new ChatMessage { Id = messageId, ChatSessionId = sessionId, SenderId = userId };
            _repositoryMock.Setup(r => r.GetChatMessageByIdAndSessionAsync(messageId, sessionId, userId)).ReturnsAsync(message);
            var command = new EditMessageCommand(sessionId, messageId, "", userId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Message text cannot be empty", exception.Message); // Giả định logic xử lý lỗi
        }
    }
}
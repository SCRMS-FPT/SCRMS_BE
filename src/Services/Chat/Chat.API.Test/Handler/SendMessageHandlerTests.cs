using Chat.API.Data;
using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Chat.API.Features.SendMessage;
using Moq;
using Xunit;

namespace Chat.API.Tests.Handler
{
    public class SendMessageHandlerTests
    {
        private readonly Mock<IChatMessageRepository> _repositoryMock;
        private readonly SendMessageHandler _handler;

        public SendMessageHandlerTests()
        {
            _repositoryMock = new Mock<IChatMessageRepository>();
            _handler = new SendMessageHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_SendsMessage_WhenValid()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var command = new SendMessageCommand(sessionId, senderId, "Hello");
            _repositoryMock.Setup(r => r.AddChatMessageAsync(It.IsAny<ChatMessage>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hello", result.MessageText);
            Assert.Equal(sessionId, result.ChatSessionId);
            Assert.Equal(senderId, result.SenderId);
            _repositoryMock.Verify(r => r.AddChatMessageAsync(It.IsAny<ChatMessage>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenMessageTextIsEmpty()
        {
            // Arrange
            var command = new SendMessageCommand(Guid.NewGuid(), Guid.NewGuid(), "");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Message text cannot be empty", exception.Message); // Giả định logic xử lý lỗi
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenChatSessionIdIsEmpty()
        {
            // Arrange
            var command = new SendMessageCommand(Guid.Empty, Guid.NewGuid(), "Hello");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("ChatSessionId cannot be empty", exception.Message); // Giả định logic xử lý lỗi
        }
    }
}
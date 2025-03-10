using Chat.API.Data;
using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Chat.API.Features.MarkMessageAsRead;
using Moq;
using Xunit;

namespace Chat.API.Tests.Handler
{
    public class MarkMessageAsReadHandlerTests
    {
        private readonly Mock<IChatMessageRepository> _repositoryMock;
        private readonly MarkMessageAsReadHandler _handler;

        public MarkMessageAsReadHandlerTests()
        {
            _repositoryMock = new Mock<IChatMessageRepository>();
            _handler = new MarkMessageAsReadHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_MarksMessageAsRead_WhenMessageExists()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var message = new ChatMessage { Id = messageId };
            _repositoryMock.Setup(r => r.GetChatMessageByIdAsync(messageId)).ReturnsAsync(message);
            var command = new MarkMessageAsReadCommand(Guid.NewGuid(), messageId, Guid.NewGuid());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(message.ReadAt);
            _repositoryMock.Verify(r => r.UpdateChatMessageAsync(message), Times.Once);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenMessageNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetChatMessageByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ChatMessage)null);
            var command = new MarkMessageAsReadCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Message not found", exception.Message);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenMessageIdIsEmpty()
        {
            // Arrange
            var command = new MarkMessageAsReadCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("MessageId cannot be empty", exception.Message); // Giả định logic xử lý lỗi
        }
    }
}
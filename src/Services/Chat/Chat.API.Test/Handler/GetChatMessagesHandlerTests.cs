using Chat.API.Data;
using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Chat.API.Features.GetChatMessages;
using Moq;
using Xunit;

namespace Chat.API.Tests.Handler
{
    public class GetChatMessagesHandlerTests
    {
        private readonly Mock<IChatMessageRepository> _repositoryMock;
        private readonly GetChatMessagesHandler _handler;

        public GetChatMessagesHandlerTests()
        {
            _repositoryMock = new Mock<IChatMessageRepository>();
            _handler = new GetChatMessagesHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsMessages_WhenValidPagination()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var messages = new List<ChatMessage> { new ChatMessage(), new ChatMessage() };
            _repositoryMock.Setup(r => r.GetChatMessageByChatSessionIdAsync(sessionId, 1, 10)).ReturnsAsync(messages);
            var query = new GetChatMessagesQuery(sessionId, 1, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(messages, result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoMessages()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetChatMessageByChatSessionIdAsync(sessionId, 1, 10)).ReturnsAsync(new List<ChatMessage>());
            var query = new GetChatMessagesQuery(sessionId, 1, 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenPageIsZero()
        {
            // Arrange
            var query = new GetChatMessagesQuery(Guid.NewGuid(), 0, 10);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
            Assert.Equal("Page must be greater than 0", exception.Message); // Giả định logic xử lý lỗi
        }
    }
}
using Chat.API.Data;
using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Chat.API.Features.SendMessage;
using Chat.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Chat.API.Tests.Handler
{
    public class SendMessageHandlerTests
    {
        private readonly Mock<IChatMessageRepository> _repositoryMock;
        private readonly Mock<IHubContext<ChatHub>> _hubContextMock;
        private readonly SendMessageHandler _handler;

        public SendMessageHandlerTests()
        {
            _repositoryMock = new Mock<IChatMessageRepository>();
            _hubContextMock = new Mock<IHubContext<ChatHub>>();
            _handler = new SendMessageHandler(_repositoryMock.Object, _hubContextMock.Object);
        }

        [Fact]
        public async Task Handle_SendsMessage_WhenValid()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var command = new SendMessageCommand(sessionId, senderId, "Hello");
            _repositoryMock.Setup(r => r.AddChatMessageAsync(It.IsAny<ChatMessage>())).Returns(Task.CompletedTask);

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(sessionId.ToString())).Returns(clientProxyMock.Object);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hello", result.MessageText);
            Assert.Equal(sessionId, result.ChatSessionId);
            Assert.Equal(senderId, result.SenderId);
            _repositoryMock.Verify(r => r.AddChatMessageAsync(It.IsAny<ChatMessage>()), Times.Once);
            clientProxyMock.Verify(c => c.SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenMessageTextIsEmpty()
        {
            // Arrange
            var command = new SendMessageCommand(Guid.NewGuid(), Guid.NewGuid(), "");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Message text cannot be empty", exception.Message);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenChatSessionIdIsEmpty()
        {
            // Arrange
            var command = new SendMessageCommand(Guid.Empty, Guid.NewGuid(), "Hello");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("ChatSessionId cannot be empty", exception.Message);
        }
    }
}
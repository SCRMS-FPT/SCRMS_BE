using Chat.API.Data;
using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chat.API.Tests
{
    public class ChatMessageRepositoryTests
    {
        private DbContextOptions<ChatDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetChatMessageByIdAsync_MessageExists_ReturnsMessage()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                MessageText = "Test message",
                SentAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();
            var repository = new ChatMessageRepository(context);

            // Act
            var result = await repository.GetChatMessageByIdAsync(message.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(message.Id, result.Id);
            Assert.Equal(message.MessageText, result.MessageText);
        }

        [Fact]
        public async Task GetChatMessageByIdAsync_MessageDoesNotExist_ReturnsNull()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var repository = new ChatMessageRepository(context);

            // Act
            var result = await repository.GetChatMessageByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetChatMessageByIdAndSessionAsync_MessageMatches_ReturnsMessage()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                MessageText = "Test",
                SentAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();
            var repository = new ChatMessageRepository(context);

            // Act
            var result = await repository.GetChatMessageByIdAndSessionAsync(message.Id, message.ChatSessionId, message.SenderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(message.Id, result.Id);
        }

        [Fact]
        public async Task GetChatMessageByIdAndSessionAsync_MessageDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                MessageText = "Test",
                SentAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();
            var repository = new ChatMessageRepository(context);

            // Act
            var result = await repository.GetChatMessageByIdAndSessionAsync(message.Id, Guid.NewGuid(), message.SenderId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetChatMessageByChatSessionIdAsync_ReturnsPaginatedOrderedMessages()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var chatSessionId = Guid.NewGuid();
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Id = Guid.NewGuid(), ChatSessionId = chatSessionId, SenderId = Guid.NewGuid(), MessageText = "Msg1", SentAt = DateTime.UtcNow.AddMinutes(-2), UpdatedAt = DateTime.UtcNow },
                new ChatMessage { Id = Guid.NewGuid(), ChatSessionId = chatSessionId, SenderId = Guid.NewGuid(), MessageText = "Msg2", SentAt = DateTime.UtcNow.AddMinutes(-1), UpdatedAt = DateTime.UtcNow },
                new ChatMessage { Id = Guid.NewGuid(), ChatSessionId = chatSessionId, SenderId = Guid.NewGuid(), MessageText = "Msg3", SentAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.ChatMessages.AddRange(messages);
            await context.SaveChangesAsync();
            var repository = new ChatMessageRepository(context);

            // Act
            var result = await repository.GetChatMessageByChatSessionIdAsync(chatSessionId, 1, 2);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(messages[2].Id, result[0].Id); // Newest first (Msg3)
            Assert.Equal(messages[1].Id, result[1].Id); // Then Msg2
        }

        [Fact]
        public async Task GetChatMessageByChatSessionIdAsync_NoMessages_ReturnsEmptyList()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var repository = new ChatMessageRepository(context);

            // Act
            var result = await repository.GetChatMessageByChatSessionIdAsync(Guid.NewGuid(), 1, 10);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddChatMessageAsync_AddsMessageSuccessfully()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var repository = new ChatMessageRepository(context);
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                MessageText = "New message",
                SentAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            await repository.AddChatMessageAsync(message);
            var addedMessage = await context.ChatMessages.FindAsync(message.Id);

            // Assert
            Assert.NotNull(addedMessage);
            Assert.Equal(message.MessageText, addedMessage.MessageText);
        }

        [Fact]
        public async Task UpdateChatMessageAsync_UpdatesMessageSuccessfully()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                MessageText = "Original",
                SentAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();
            var repository = new ChatMessageRepository(context);

            // Act
            message.MessageText = "Updated";
            await repository.UpdateChatMessageAsync(message);
            var updatedMessage = await context.ChatMessages.FindAsync(message.Id);

            // Assert
            Assert.Equal("Updated", updatedMessage.MessageText);
        }
    }
}
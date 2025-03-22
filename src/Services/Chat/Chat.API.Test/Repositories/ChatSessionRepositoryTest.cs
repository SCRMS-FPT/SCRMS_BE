using Chat.API.Data;
using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chat.API.Tests
{
    public class ChatSessionRepositoryTests
    {
        private DbContextOptions<ChatDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ChatDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetChatSessionByIdAsync_SessionExists_ReturnsSession()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var session = new ChatSession
            {
                Id = Guid.NewGuid(),
                User1Id = Guid.NewGuid(),
                User2Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ChatSessions.Add(session);
            await context.SaveChangesAsync();
            var repository = new ChatSessionRepository(context);

            // Act
            var result = await repository.GetChatSessionByIdAsync(session.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
        }

        [Fact]
        public async Task GetChatSessionByIdAsync_SessionDoesNotExist_ReturnsNull()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var repository = new ChatSessionRepository(context);

            // Act
            var result = await repository.GetChatSessionByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetChatSessionByUsersAsync_SessionExists_ReturnsSession()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var session = new ChatSession
            {
                Id = Guid.NewGuid(),
                User1Id = user1Id,
                User2Id = user2Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ChatSessions.Add(session);
            await context.SaveChangesAsync();
            var repository = new ChatSessionRepository(context);

            // Act
            var result = await repository.GetChatSessionByUsersAsync(user1Id, user2Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
        }

        [Fact]
        public async Task GetChatSessionByUsersAsync_SessionExistsReverseOrder_ReturnsSession()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var session = new ChatSession
            {
                Id = Guid.NewGuid(),
                User1Id = user1Id,
                User2Id = user2Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ChatSessions.Add(session);
            await context.SaveChangesAsync();
            var repository = new ChatSessionRepository(context);

            // Act
            var result = await repository.GetChatSessionByUsersAsync(user2Id, user1Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
        }

        [Fact]
        public async Task AddChatSessionAsync_AddsSessionSuccessfully()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var repository = new ChatSessionRepository(context);
            var session = new ChatSession
            {
                Id = Guid.NewGuid(),
                User1Id = Guid.NewGuid(),
                User2Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            await repository.AddChatSessionAsync(session);
            var addedSession = await context.ChatSessions.FindAsync(session.Id);

            // Assert
            Assert.NotNull(addedSession);
            Assert.Equal(session.User1Id, addedSession.User1Id);
        }

        [Fact]
        public async Task GetChatSessionByUserIdAsync_ReturnsSessionsForUser()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            using var context = new ChatDbContext(options);
            var userId = Guid.NewGuid();
            var sessions = new List<ChatSession>
            {
                new ChatSession { Id = Guid.NewGuid(), User1Id = userId, User2Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ChatSession { Id = Guid.NewGuid(), User1Id = Guid.NewGuid(), User2Id = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.ChatSessions.AddRange(sessions);
            await context.SaveChangesAsync();
            var repository = new ChatSessionRepository(context);

            // Act
            var result = await repository.GetChatSessionByUserIdAsync(userId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Id == sessions[0].Id);
            Assert.Contains(result, r => r.Id == sessions[1].Id);
        }
    }
}
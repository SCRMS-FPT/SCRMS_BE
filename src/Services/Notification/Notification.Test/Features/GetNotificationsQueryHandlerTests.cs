using Notification.API.Data.Model;
using Notification.API.Features.GetNotifications;
using Xunit.Abstractions;

namespace Notification.Test.Features
{
    public class GetNotificationsQueryHandlerTests : HandlerTestBase
    {
        private readonly GetNotificationsQueryHandler _handler;

        public GetNotificationsQueryHandlerTests() : base()
        {
            _handler = new GetNotificationsQueryHandler(Context, MediatorMock.Object);
        }

        [Fact]
        public async Task Handle_RetrievesAllNotificationsForUser()
        {
            // Arrange
            Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var notifications = new List<MessageNotification>
            {
                new MessageNotification { Receiver = userId, Title = "Hardcoded Title 1", IsRead = false, Content = "Hardcoded Content 1", Type = "Hardcoded Type 1" },
                new MessageNotification { Receiver = userId, Title = "Hardcoded Title 2", IsRead = true, Content = "Hardcoded Content 2", Type = "Hardcoded Type 2" }
            };

            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();
            var query = new GetNotificationsQuery(userId, 1, 10, null, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result.Data, n => n.Title == "Hardcoded Title 1" && !n.IsRead);
            Assert.Contains(result.Data, n => n.Title == "Hardcoded Title 2" && n.IsRead);
        }

        [Fact]
        public async Task Handle_FiltersNotificationsByIsRead()
        {
            // Arrange
            Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var notifications = new List<MessageNotification>
            {
                new MessageNotification { Receiver = userId, Title = "Hardcoded Title 3", IsRead = false, Content = "Hardcoded Content 3", Type = "Hardcoded Type 3" },
                new MessageNotification { Receiver = userId, Title = "Hardcoded Title 4", IsRead = true, Content = "Hardcoded Content 4", Type = "Hardcoded Type 4" }
            };

            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();
            var query = new GetNotificationsQuery(userId, 1, 10, true, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Data);
            Assert.True(result.Data.First().IsRead);
        }

        [Fact]
        public async Task Handle_FiltersNotificationsByType()
        {
            // Arrange
            Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000003");
            var notifications = new List<MessageNotification>
            {
                new MessageNotification { Receiver = userId, Title = "Hardcoded Title 5", Type = "Info", Content = "Hardcoded Content 5" },
                new MessageNotification { Receiver = userId, Title = "Hardcoded Title 6", Type = "Alert", Content = "Hardcoded Content 6" }
            };
            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();
            var query = new GetNotificationsQuery(userId, 1, 10, null, "Info");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Info", result.Data.First().Type);
        }

        [Fact]
        public async Task Handle_AppliesPaginationCorrectly()
        {
            // Arrange
            Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000004");
            var notifications = Enumerable.Range(1, 5).Select(i => new MessageNotification
            {
                Receiver = userId,
                Title = $"Hardcoded Notification {i}",
                Type = "Test",
                Content = "Hardcoded Content"
            }).ToList();

            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();
            var query = new GetNotificationsQuery(userId, 2, 2, null, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal("Hardcoded Notification 3", result.Data.First().Title);
            Assert.Equal("Hardcoded Notification 2", result.Data.Last().Title);
        }
    }
}
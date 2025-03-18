using Notification.API.Data.Model;
using Notification.API.Features.GetNotifications;

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
            var userId = Guid.NewGuid();
            var notifications = new List<MessageNotification>
        {
            new() { Receiver = userId, Title = "Notif1", IsRead = false,  Content = "This is an test" , Type = "Info" },
            new() { Receiver = userId, Title = "Notif2", IsRead = true, Content = "Alert test", Type = "Alert"}
        };
            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();
            var query = new GetNotificationsQuery(userId, 1, 10, null, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count); // Verifies the correct number of notifications
            Assert.Contains(result, n => n.Title == "Notif1" && !n.IsRead); // Verifies specific notification properties
            Assert.Contains(result, n => n.Title == "Notif2" && n.IsRead);
        }

        [Fact]
        public async Task Handle_FiltersNotificationsByIsRead()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notifications = new List<MessageNotification>
        {
            new() { Receiver = userId, Title = "Notif1", IsRead = false, Content = "This is not read", Type = "Info" },
            new() { Receiver = userId, Title = "Notif2", IsRead = true, Content = "This is read", Type = "Info" }
        };
            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();
            var query = new GetNotificationsQuery(userId, 1, 10, true, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result); // Verifies only one notification is returned
            Assert.True(result[0].IsRead); // Verifies it’s the read notification
        }

        [Fact]
        public async Task Handle_FiltersNotificationsByType()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notifications = new List<MessageNotification>
        {
            new() { Receiver = userId, Title = "Notif1", Type = "Info", Content = "Info test" },
            new() { Receiver = userId, Title = "Notif2", Type = "Alert", Content = "Alert test" }
        };
            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();
            var query = new GetNotificationsQuery(userId, 1, 10, null, "Info");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result); // Verifies only one notification is returned
            Assert.Equal("Info", result[0].Type); // Verifies the correct type
        }

        [Fact]
        public async Task Handle_AppliesPaginationCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notifications = Enumerable.Range(1, 5).Select(i => new MessageNotification
            {
                Receiver = userId,
                Title = $"Notification number {i}",
                Type = "Test",
                Content = "This is an test"
            }).ToList();
            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();
            var query = new GetNotificationsQuery(userId, 2, 2, null, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count); // Verifies pagination limits the result to 2 items
        }
    }
}

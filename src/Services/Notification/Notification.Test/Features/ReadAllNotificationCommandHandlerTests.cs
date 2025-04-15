using Microsoft.EntityFrameworkCore;
using Notification.API.Data.Model;
using Notification.API.Features.ReadAllNotification;

namespace Notification.Test.Features
{
    public class ReadAllNotificationCommandHandlerTests : HandlerTestBase
    {
        private readonly ReadAllNotificationCommandHandler _handler;
        private readonly Guid _hardcodedUserId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        private readonly Guid _hardcodedOtherUserId = Guid.Parse("00000000-0000-0000-0000-000000000011");

        public ReadAllNotificationCommandHandlerTests()
        {
            _handler = new ReadAllNotificationCommandHandler(Context, MediatorMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_MarksNotificationsAsRead()
        {
            // Arrange
            var notifications = new List<MessageNotification>
            {
                new MessageNotification
                {
                    Id = Guid.NewGuid(),
                    Receiver = _hardcodedUserId,
                    IsRead = false,
                    Title = "Hardcoded Title 1",
                    Content = "Hardcoded Content 1",
                    Type = "Hardcoded Type 1"
                },
                new MessageNotification
                {
                    Id = Guid.NewGuid(),
                    Receiver = _hardcodedUserId,
                    IsRead = false,
                    Title = "Hardcoded Title 2",
                    Content = "Hardcoded Content 2",
                    Type = "Hardcoded Type 2"
                },
                new MessageNotification
                {
                    Id = Guid.NewGuid(),
                    Receiver = _hardcodedUserId,
                    IsRead = false,
                    Title = "Hardcoded Title 3",
                    Content = "Hardcoded Content 3",
                    Type = "Hardcoded Type 3"
                }
            };
            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();

            var command = new ReadAllNotificationCommand(_hardcodedUserId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedNotifications = await Context.MessageNotifications.ToListAsync();
            Assert.All(updatedNotifications, n => Assert.True(n.IsRead));
        }

        [Fact]
        public async Task Handle_NoUnreadNotifications_DoesNotModifyContext()
        {
            // Arrange
            var notifications = new List<MessageNotification>
            {
                new MessageNotification
                {
                    Id = Guid.NewGuid(),
                    Receiver = _hardcodedUserId,
                    IsRead = true,
                    Title = "Hardcoded Read Title 1",
                    Content = "Hardcoded Read Content 1",
                    Type = "Hardcoded Read Type 1"
                },
                new MessageNotification
                {
                    Id = Guid.NewGuid(),
                    Receiver = _hardcodedUserId,
                    IsRead = true,
                    Title = "Hardcoded Read Title 2",
                    Content = "Hardcoded Read Content 2",
                    Type = "Hardcoded Read Type 2"
                }
            };

            Context.MessageNotifications.AddRange(notifications);
            await Context.SaveChangesAsync();

            var command = new ReadAllNotificationCommand(_hardcodedUserId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedNotifications = await Context.MessageNotifications.ToListAsync();
            Assert.All(updatedNotifications, n => Assert.True(n.IsRead));
            Assert.Equal(2, updatedNotifications.Count);
        }

        [Fact]
        public async Task Handle_NullNotificationList_DoesNotModifyContext()
        {
            // Arrange
            var command = new ReadAllNotificationCommand(_hardcodedUserId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedNotifications = await Context.MessageNotifications.ToListAsync();
            Assert.Empty(updatedNotifications);
        }
    }
}
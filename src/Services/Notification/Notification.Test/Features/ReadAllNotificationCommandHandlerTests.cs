using Microsoft.EntityFrameworkCore;
using Notification.API.Data.Model;
using Notification.API.Features.ReadAllNotification;

namespace Notification.Test.Features
{
    public class ReadAllNotificationCommandHandlerTests : HandlerTestBase
    {
        private readonly ReadAllNotificationCommandHandler _handler;
        private readonly Guid _hardcodedUserId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        private readonly Guid _testId1 = Guid.Parse("00000000-0000-0000-0000-000000000011");
        private readonly Guid _testId2 = Guid.Parse("00000000-0000-0000-0000-000000000012");
        private readonly Guid _testId3 = Guid.Parse("00000000-0000-0000-0000-000000000013");

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
                    Id = _testId1,
                    Receiver = _hardcodedUserId,
                    IsRead = false,
                    Title = "Hardcoded Title 1",
                    Content = "Hardcoded Content 1",
                    Type = "Court"
                },
                new MessageNotification
                {
                    Id = _testId2,
                    Receiver = _hardcodedUserId,
                    IsRead = false,
                    Title = "Hardcoded Title 2",
                    Content = "Hardcoded Content 2",
                    Type = "Court"
                },
                new MessageNotification
                {
                    Id = _testId3,
                    Receiver = _hardcodedUserId,
                    IsRead = false,
                    Title = "Hardcoded Title 3",
                    Content = "Hardcoded Content 3",
                    Type = "Court"
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
                    Id = _testId1,
                    Receiver = _hardcodedUserId,
                    IsRead = true,
                    Title = "Hardcoded Read Title 1",
                    Content = "Hardcoded Read Content 1",
                    Type = "Coach"
                },
                new MessageNotification
                {
                    Id = _testId2,
                    Receiver = _hardcodedUserId,
                    IsRead = true,
                    Title = "Hardcoded Read Title 2",
                    Content = "Hardcoded Read Content 2",
                    Type = "Coach"
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
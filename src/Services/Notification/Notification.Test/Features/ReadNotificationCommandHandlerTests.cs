using BuildingBlocks.Exceptions;
using Notification.API.Data.Model;
using Notification.API.Features.ReadNotification;

namespace Notification.Test.Features
{
    public class ReadNotificationCommandHandlerTests : HandlerTestBase
    {
        private readonly ReadNotificationsCommandHandler _handler;

        public ReadNotificationCommandHandlerTests() : base()
        {
            _handler = new ReadNotificationsCommandHandler(Context, MediatorMock.Object);
        }

        [Fact]
        public async Task Handle_SetsIsReadToTrue_WhenNotificationExistsAndBelongsToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notificationId = Guid.NewGuid();
            var notification = new MessageNotification { Id = notificationId, Receiver = userId, IsRead = false, Content = "Testing", Type = "Test", Title = "Testing title" };
            Context.MessageNotifications.Add(notification);
            await Context.SaveChangesAsync();
            var command = new ReadNotificationCommand(notificationId, userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedNotification = await Context.MessageNotifications.FindAsync(notificationId);
            Assert.True(updatedNotification.IsRead); // Verifies the notification is marked as read
        }

        [Fact]
        public async Task Handle_ThrowsNotFoundException_WhenNotificationDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notificationId = Guid.NewGuid();
            var command = new ReadNotificationCommand(notificationId, userId);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ThrowsBadRequestException_WhenNotificationBelongsToAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var notificationId = Guid.NewGuid();
            var notification = new MessageNotification { Id = notificationId, Receiver = anotherUserId, IsRead = false, Type = "Info", Content = "This is an info test", Title = "Testing title" };
            Context.MessageNotifications.Add(notification);
            await Context.SaveChangesAsync();
            var command = new ReadNotificationCommand(notificationId, userId);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}

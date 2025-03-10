using BuildingBlocks.Exceptions;
using Notification.API.Data.Model;
using Notification.API.Features.DeleteNotification;

namespace Notification.Test.Features
{
    public class DeleteNotificationCommandHandlerTests : HandlerTestBase
    {
        private readonly DeleteNotificationCommandHandler _handler;

        public DeleteNotificationCommandHandlerTests() : base()
        {
            _handler = new DeleteNotificationCommandHandler(Context, MediatorMock.Object);
        }

        [Fact]
        public async Task Handle_DeletesNotification_WhenNotificationExistsAndBelongsToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notificationId = Guid.NewGuid();
            var notification = new MessageNotification { Id = notificationId, Receiver = userId, Title = "Test", Content = "Test", Type = "Info" };
            Context.MessageNotifications.Add(notification);
            await Context.SaveChangesAsync();
            var command = new DeleteNotificationCommand(notificationId, userId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var deletedNotification = await Context.MessageNotifications.FindAsync(notificationId);
            Assert.Null(deletedNotification);
        }

        [Fact]
        public async Task Handle_ThrowsNotFoundException_WhenNotificationDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notificationId = Guid.NewGuid();
            var command = new DeleteNotificationCommand(notificationId, userId);

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

            // Create the notification with all required properties
            var notification = new MessageNotification
            {
                Id = notificationId,
                Receiver = anotherUserId,
                Title = "Test",
                Content = "Test content",  // Required property
                Type = "Info",            // Required property
                IsRead = false            // Optional, but included for completeness
            };

            // Add to context and save
            Context.MessageNotifications.Add(notification);
            await Context.SaveChangesAsync();

            // Set up the command
            var command = new DeleteNotificationCommand(notificationId, userId);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}

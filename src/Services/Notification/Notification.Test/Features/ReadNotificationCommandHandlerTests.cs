using BuildingBlocks.Exceptions;
using Notification.API.Data.Model;
using Notification.API.Features.ReadNotification;

namespace Notification.Test.Features
{
    public class ReadNotificationCommandHandlerTests : HandlerTestBase
    {
        private readonly ReadNotificationsCommandHandler _handler;
        private readonly Guid _hardcodedUserId = Guid.Parse("00000000-0000-0000-0000-000000000020");
        private readonly Guid _hardcodedNotificationId = Guid.Parse("00000000-0000-0000-0000-000000000021");
        private readonly Guid _hardcodedAnotherUserId = Guid.Parse("00000000-0000-0000-0000-000000000022");

        public ReadNotificationCommandHandlerTests() : base()
        {
            _handler = new ReadNotificationsCommandHandler(Context, MediatorMock.Object);
        }

        [Fact]
        public async Task Handle_SetsIsReadToTrue_WhenNotificationExistsAndBelongsToUser()
        {
            // Arrange
            var notification = new MessageNotification
            {
                Id = _hardcodedNotificationId,
                Receiver = _hardcodedUserId,
                IsRead = false,
                Content = "Hardcoded Testing Content",
                Type = "Hardcoded Test Type",
                Title = "Hardcoded Testing Title"
            };
            Context.MessageNotifications.Add(notification);
            await Context.SaveChangesAsync();
            var command = new ReadNotificationCommand(_hardcodedNotificationId, _hardcodedUserId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedNotification = await Context.MessageNotifications.FindAsync(_hardcodedNotificationId);
            Assert.True(updatedNotification.IsRead);
        }

        [Fact]
        public async Task Handle_ThrowsNotFoundException_WhenNotificationDoesNotExist()
        {
            // Arrange
            var command = new ReadNotificationCommand(_hardcodedNotificationId, _hardcodedUserId); // Using an existing hardcoded ID

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ThrowsBadRequestException_WhenNotificationBelongsToAnotherUser()
        {
            // Arrange
            var notification = new MessageNotification
            {
                Id = _hardcodedNotificationId,
                Receiver = _hardcodedAnotherUserId,
                IsRead = false,
                Type = "Hardcoded Info Type",
                Content = "Hardcoded This is an info test",
                Title = "Hardcoded Testing title"
            };
            Context.MessageNotifications.Add(notification);
            await Context.SaveChangesAsync();
            var command = new ReadNotificationCommand(_hardcodedNotificationId, _hardcodedUserId);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
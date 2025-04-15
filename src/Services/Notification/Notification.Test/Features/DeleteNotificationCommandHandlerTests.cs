using BuildingBlocks.Exceptions;
using Notification.API.Features.DeleteNotification;

namespace Notification.Test.Features
{
    public class DeleteNotificationCommandHandlerTests : HandlerTestBase
    {
        private readonly DeleteNotificationCommandHandler _handler;
        private readonly Guid _hardcodedUserId = Guid.Parse("00000000-0000-0000-0000-000000000040");
        private readonly Guid _hardcodedNotificationId = Guid.Parse("00000000-0000-0000-0000-000000000041");
        private readonly Guid _hardcodedOtherUserId = Guid.Parse("00000000-0000-0000-0000-000000000042");

        public DeleteNotificationCommandHandlerTests() : base()
        {
            _handler = new DeleteNotificationCommandHandler(Context, MediatorMock.Object);
        }

        [Fact]
        public async Task Handle_DeletesNotification_WhenNotificationExistsAndBelongsToUser()
        {
            var notification = new Notification.API.Data.Model.MessageNotification
            {
                Id = _hardcodedNotificationId,
                Receiver = _hardcodedUserId,
                IsRead = false,
                Type = "Hardcoded Info Type",
                Content = "Hardcoded This is an info test",
                Title = "Hardcoded Testing title"
            };
            Context.MessageNotifications.Add(notification);
            await Context.SaveChangesAsync();
            var command = new DeleteNotificationCommand(_hardcodedNotificationId, _hardcodedUserId);
            await _handler.Handle(command, CancellationToken.None);
            var deletedNotification = await Context.MessageNotifications.FindAsync(_hardcodedNotificationId);
            Assert.Null(deletedNotification);
        }

        [Fact]
        public async Task Handle_ThrowsNotFoundException_WhenNotificationDoesNotExist()
        {
            var command = new DeleteNotificationCommand(_hardcodedNotificationId, _hardcodedUserId);
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ThrowsBadRequestException_WhenNotificationBelongsToAnotherUser()
        {
            var notification = new Notification.API.Data.Model.MessageNotification
            {
                Id = _hardcodedNotificationId,
                Receiver = _hardcodedOtherUserId,
                IsRead = false,
                Type = "Hardcoded Info Type",
                Content = "Hardcoded This is an info test",
                Title = "Hardcoded Testing title"
            };
            Context.MessageNotifications.Add(notification);
            await Context.SaveChangesAsync();
            var command = new DeleteNotificationCommand(_hardcodedNotificationId, _hardcodedUserId);
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
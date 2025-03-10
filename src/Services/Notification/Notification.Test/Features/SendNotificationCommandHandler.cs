using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Notification.Api.Features.SendEmail;
using Notification.API.Features.SendNotification;

namespace Notification.Test.Features
{
    public class SendNotificationCommandHandlerTests : HandlerTestBase
    {
        private readonly SendNotificationCommandHandler _handler;

        public SendNotificationCommandHandlerTests() : base()
        {
            _handler = new SendNotificationCommandHandler(Context, MediatorMock.Object, SenderMock.Object, HubContextMock.Object);
        }

        [Fact]
        public async Task Handle_SavesNotificationToDatabase()
        {
            var command = new SendNotificationCommand(Guid.NewGuid(), "Title", "Content", "Info", false, null);
            await _handler.Handle(command, CancellationToken.None);
            var savedNotification = await Context.MessageNotifications.FirstOrDefaultAsync(n => n.Receiver == command.SendTo);
            Assert.NotNull(savedNotification);
            Assert.Equal("Title", savedNotification.Title);
            Assert.Equal("Content", savedNotification.Content);
        }

        [Fact]
        public async Task Handle_SendsRealTimeNotificationViaSignalR()
        {
            var command = new SendNotificationCommand(Guid.NewGuid(), "Title", "Content", "Info", false, null);
            await _handler.Handle(command, CancellationToken.None);
            var clientsMock = HubContextMock.Object.Clients;
            Mock.Get(clientsMock).Verify(c => c.User(command.SendTo.ToString()), Times.Once());
        }

        [Fact]
        public async Task Handle_SendsEmail_WhenSendMailIsTrue()
        {
            var command = new SendNotificationCommand(Guid.NewGuid(), "Title", "Content", "Info", true, "test@gmail.com");
            SenderMock.Setup(s => s.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);
            await _handler.Handle(command, CancellationToken.None);
            SenderMock.Verify(s => s.Send(It.Is<SendEmailCommand>(cmd => cmd.To == command.UserEmail), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handle_DoesNotSendEmail_WhenSendMailIsFalse()
        {
            var command = new SendNotificationCommand(Guid.NewGuid(), "Title", "Content", "Info", false, "test@gmail.com");
            await _handler.Handle(command, CancellationToken.None);
            SenderMock.Verify(s => s.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}

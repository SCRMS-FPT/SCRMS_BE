using Moq;
using Notification.Api.Features.SendEmail;

namespace Notification.Test.Features
{
    public class SendEmailCommandHandlerTests : HandlerTestBase
    {
        private readonly SendEmailCommandHandler _handler;

        public SendEmailCommandHandlerTests() : base()
        {
            _handler = new SendEmailCommandHandler(EmailServiceMock.Object);
        }

        [Fact]
        public async Task Handle_CallsEmailServiceWithCorrectParameters()
        {
            // Arrange
            var command = new SendEmailCommand("test@gmail.com", "Subject", "Body", false);
            EmailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            EmailServiceMock.Verify(x => x.SendEmailAsync(command.To, command.Subject, command.Body, command.IsHtml), Times.Once());
        }
    }
}

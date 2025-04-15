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
            var hardcodedEmail = "hardcoded@example.com";
            var hardcodedSubject = "Hardcoded Subject";
            var hardcodedBody = "Hardcoded Body";
            var command = new SendEmailCommand(hardcodedEmail, hardcodedSubject, hardcodedBody, false);
            EmailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            EmailServiceMock.Verify(x => x.SendEmailAsync(hardcodedEmail, hardcodedSubject, hardcodedBody, false), Times.Once());
        }
    }
}
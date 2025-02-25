using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Identity.Commands.ChangePassword;

namespace Identity.Test.Application.Identity.Commands
{
    public class ChangePasswordHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;

        public ChangePasswordHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Handle_ShouldChangePasswordSuccessfully()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), UserName = "test@example.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "oldPass", "newPass"))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new ChangePasswordHandler(_userManagerMock.Object);
            var command = new ChangePasswordCommand(user.Id, "oldPass", "newPass");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _userManagerMock.Verify(x => x.ChangePasswordAsync(user, "oldPass", "newPass"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var handler = new ChangePasswordHandler(_userManagerMock.Object);
            var command = new ChangePasswordCommand(Guid.NewGuid(), "oldPass", "newPass");

            // Act
            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("User not found");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenChangePasswordFails()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), UserName = "test@example.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Error changing password" });
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "oldPass", "newPass"))
                .ReturnsAsync(failedResult);

            var handler = new ChangePasswordHandler(_userManagerMock.Object);
            var command = new ChangePasswordCommand(user.Id, "oldPass", "newPass");

            // Act
            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Failed to change password: Error changing password");
        }
    }
}
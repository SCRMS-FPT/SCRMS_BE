using Identity.Application.Identity.Commands.Register;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Test.Application.Identity.Commands
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;

        public RegisterUserHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Handle_ShouldRegisterUserSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new RegisterUserCommand("First", "Last", "test@example.com", "+1234567890", DateTime.UtcNow.AddYears(-20), "Male", "Password123");
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), "Password123"))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((user, pass) => user.Id = userId);

            var handler = new RegisterUserHandler(_userManagerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Id.Should().Be(userId);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), "Password123"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserCreationFails()
        {
            // Arrange
            var command = new RegisterUserCommand("First", "Last", "test@example.com", "+1234567890", DateTime.UtcNow.AddYears(-20), "Male", "Password123");
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), "Password123"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

            var handler = new RegisterUserHandler(_userManagerMock.Object);

            // Act
            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Failed to create user: Creation failed");
        }
    }
}
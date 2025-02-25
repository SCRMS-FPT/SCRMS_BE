using Identity.Application.Extensions;
using Identity.Application.Identity.Commands.Login;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Test.Application.Identity.Commands
{
    public class LoginUserHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock; private readonly IOptions<JwtSettings> _jwtSettings;

        public LoginUserHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            _jwtSettings = Options.Create(new JwtSettings
            {
                Secret = "TestSecretKey123456789012345678901234",  // 32 ký tự = 256 bit
                ExpiryHours = 1,
                Issuer = "identity-service",
                Audience = "webapp"
            });
        }

        [Fact]
        public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "test@example.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "password"))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            var handler = new LoginUserHandler(_userManagerMock.Object, _jwtSettings);
            var command = new LoginUserCommand("test@example.com", "password");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("test@example.com");
            result.UserId.Should().Be(user.Id);
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenCredentialsAreInvalid()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync("wrong@example.com"))
                .ReturnsAsync((User)null);

            var handler = new LoginUserHandler(_userManagerMock.Object, _jwtSettings);
            var command = new LoginUserCommand("wrong@example.com", "password");

            // Act
            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Invalid credentials");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Identity.Commands.Role;

namespace Identity.Test.Application.Identity.Commands
{
    public class AssignRolesToUserHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock; private readonly Mock<RoleManager<IdentityRole<Guid>>> _roleManagerMock;

        public AssignRolesToUserHandlerTests()
        {
            var userStore = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole<Guid>>>(roleStore.Object, null, null, null, null);
        }

        [Fact]
        public async Task Handle_ShouldAssignRolesSuccessfully()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), UserName = "test@example.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
            _roleManagerMock.Setup(x => x.RoleExistsAsync("Admin"))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new AssignRolesToUserHandler(_userManagerMock.Object, _roleManagerMock.Object);
            var command = new AssignRolesToUserCommand(user.Id, new List<string> { "Admin" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _userManagerMock.Verify(x => x.AddToRolesAsync(user, It.Is<IEnumerable<string>>(roles => roles.Contains("Admin"))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var handler = new AssignRolesToUserHandler(_userManagerMock.Object, _roleManagerMock.Object);
            var command = new AssignRolesToUserCommand(Guid.NewGuid(), new List<string> { "Admin" });

            // Act
            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("User not found");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRoleDoesNotExist()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), UserName = "test@example.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
            _roleManagerMock.Setup(x => x.RoleExistsAsync("NonExistentRole"))
                .ReturnsAsync(false);

            var handler = new AssignRolesToUserHandler(_userManagerMock.Object, _roleManagerMock.Object);
            var command = new AssignRolesToUserCommand(user.Id, new List<string> { "NonExistentRole" });

            // Act
            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Role 'NonExistentRole' does not exist");
        }
    }
}
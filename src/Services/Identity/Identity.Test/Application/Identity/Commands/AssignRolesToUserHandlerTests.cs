using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Identity.Commands.Role;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using Identity.Domain.Models;
using MediatR;
using Moq;
using Xunit;

namespace Identity.Test.Application.Identity.Commands
{
    public class AssignRolesToUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public AssignRolesToUserHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task Handle_ShouldAssignRolesSuccessfully()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), UserName = "test@example.com" };
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _userRepositoryMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            _userRepositoryMock.Setup(x => x.AddToRolesAsync(user, It.Is<IEnumerable<string>>(roles => roles.Contains("Admin"))))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new AssignRolesToUserHandler(_userRepositoryMock.Object);
            var command = new AssignRolesToUserCommand(user.Id, new List<string> { "Admin" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _userRepositoryMock.Verify(x => x.AddToRolesAsync(user, It.Is<IEnumerable<string>>(roles => roles.Contains("Admin"))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User)null);
            var handler = new AssignRolesToUserHandler(_userRepositoryMock.Object);
            var command = new AssignRolesToUserCommand(Guid.NewGuid(), new List<string> { "Admin" });

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("User not found");
        }
    }
}
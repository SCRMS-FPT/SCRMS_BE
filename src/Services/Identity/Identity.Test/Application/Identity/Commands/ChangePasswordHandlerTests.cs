using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Identity.Commands.ChangePassword;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using Identity.Domain.Models;
using MediatR;
using Moq;
using Xunit;

namespace Identity.Test.Application.Identity.Commands
{
    public class ChangePasswordHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public ChangePasswordHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task Handle_ShouldChangePasswordSuccessfully()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), UserName = "test@example.com" };
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdatePasswordAsync(user, "oldPass", "newPass"))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new ChangePasswordHandler(_userRepositoryMock.Object);
            var command = new ChangePasswordCommand(user.Id, "oldPass", "newPass");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _userRepositoryMock.Verify(x => x.UpdatePasswordAsync(user, "oldPass", "newPass"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User)null);
            var handler = new ChangePasswordHandler(_userRepositoryMock.Object);
            var command = new ChangePasswordCommand(Guid.NewGuid(), "oldPass", "newPass");

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("User not found");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenChangePasswordFails()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), UserName = "test@example.com" };
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Error changing password" });
            _userRepositoryMock.Setup(x => x.UpdatePasswordAsync(user, "oldPass", "newPass"))
                .ReturnsAsync(failedResult);

            var handler = new ChangePasswordHandler(_userRepositoryMock.Object);
            var command = new ChangePasswordCommand(user.Id, "oldPass", "newPass");

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Failed to change password: Error changing password");
        }
    }
}
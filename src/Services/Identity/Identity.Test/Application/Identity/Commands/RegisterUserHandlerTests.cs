using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Identity.Commands.Register;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using Identity.Domain.Models;
using MediatR;
using Moq;
using Xunit;

namespace Identity.Test.Application.Identity.Commands
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public RegisterUserHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task Handle_ShouldRegisterUserSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new RegisterUserCommand("First", "Last", "test@example.com", "+1234567890", DateTime.UtcNow.AddYears(-20), "Male", "Password123");
            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>(), "Password123"))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((user, pass) => user.Id = userId);

            var handler = new RegisterUserHandler(_userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Id.Should().Be(userId);
            _userRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>(), "Password123"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserCreationFails()
        {
            // Arrange
            var command = new RegisterUserCommand("First", "Last", "test@example.com", "+1234567890", DateTime.UtcNow.AddYears(-20), "Male", "Password123");
            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>(), "Password123"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

            var handler = new RegisterUserHandler(_userRepositoryMock.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Failed to create user: Creation failed");
        }
    }
}
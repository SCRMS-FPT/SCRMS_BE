using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Identity.Commands.UpdateProfile;
using Identity.Application.Dtos;
using Identity.Domain.Exceptions;
using Identity.Domain.Models;
using MediatR;
using Moq;
using Xunit;
using Identity.Application.Data.Repositories;

namespace Identity.Test.Application.Identity.Commands
{
    public class UpdateProfileHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public UpdateProfileHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task Handle_ShouldUpdateProfileSuccessfully()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Old",
                LastName = "Name",
                PhoneNumber = "123",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                Gender = Gender.Male,
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            };
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdateUserAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new UpdateProfileHandler(_userRepositoryMock.Object);
            var command = new UpdateProfileCommand(user.Id, "NewFirst", "NewLast", "+987654321", DateTime.UtcNow.AddYears(-20), "Male", "Hello!");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.FirstName.Should().Be("NewFirst");
            result.LastName.Should().Be("NewLast");
            result.Phone.Should().Be("+987654321");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User)null);
            var handler = new UpdateProfileHandler(_userRepositoryMock.Object);
            var command = new UpdateProfileCommand(Guid.NewGuid(), "NewFirst", "NewLast", "+987654321", DateTime.UtcNow.AddYears(-20), "Male", "Hello!");

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("User not found");
        }
    }
}
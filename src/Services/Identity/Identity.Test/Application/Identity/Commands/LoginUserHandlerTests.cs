using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Identity.Commands.Login;
using Identity.Application.Dtos;
using Identity.Domain.Exceptions;
using Identity.Domain.Models;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Identity.Application.Data.Repositories;
using Identity.Application.Extensions;

namespace Identity.Test.Application.Identity.Commands
{
    public class LoginUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock; private readonly IOptions<JwtSettings> _jwtSettings;

        public LoginUserHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtSettings = Options.Create(new JwtSettings
            {
                Secret = "TestSecretKey123456789012345678901234", // 32 characters (256 bits)
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
            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync("test@example.com"))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.CheckPasswordAsync(user, "password"))
                .ReturnsAsync(true);
            _userRepositoryMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            var handler = new LoginUserHandler(_userRepositoryMock.Object, _jwtSettings);
            var command = new LoginUserCommand("test@example.com", "password");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.User.Email.Should().Be("test@example.com");
            result.UserId.Should().Be(user.Id);
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenCredentialsAreInvalid()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync("wrong@example.com"))
                .ReturnsAsync((User)null);

            var handler = new LoginUserHandler(_userRepositoryMock.Object, _jwtSettings);
            var command = new LoginUserCommand("wrong@example.com", "password");

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("Invalid credentials");
        }
    }
}
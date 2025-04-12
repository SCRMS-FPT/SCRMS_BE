using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Identity.Commands.Register;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using Identity.Domain.Models;
using Identity.Application.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.Net.Http;

namespace Identity.Test.Application.Identity.Commands
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IOptions<EndpointSettings>> _endpointSettingsMock;
        private readonly EndpointSettings _endpointSettings;

        public RegisterUserHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();

            // Setup endpoint settings for email verification
            _endpointSettings = new EndpointSettings
            {
                Verification = "http://frontend/verify?token=",
                VerificationKey = "test-verification-key"
            };

            _endpointSettingsMock = new Mock<IOptions<EndpointSettings>>();
            _endpointSettingsMock.Setup(x => x.Value).Returns(_endpointSettings);
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

            // Note: Pass null for httpClientFactory to avoid HTTP calls in test
            var handler = new RegisterUserHandler(_userRepositoryMock.Object, _endpointSettingsMock.Object, null);

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

            var handler = new RegisterUserHandler(_userRepositoryMock.Object, _endpointSettingsMock.Object, null);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Failed to create user: Creation failed");
        }

        [Fact]
        public async Task Handle_ShouldSetCorrectUserProperties()
        {
            // Arrange
            User? capturedUser = null;
            var userId = Guid.NewGuid();

            var command = new RegisterUserCommand(
                "John",
                "Doe",
                "john.doe@example.com",
                "+1234567890",
                new DateTime(1995, 5, 15),
                "Male",
                "SecurePassword123"
            );

            _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((user, _) =>
                {
                    capturedUser = user;
                    user.Id = userId;
                });

            var handler = new RegisterUserHandler(_userRepositoryMock.Object, _endpointSettingsMock.Object, null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser!.FirstName.Should().Be("John");
            capturedUser.LastName.Should().Be("Doe");
            capturedUser.Email.Should().Be("john.doe@example.com");
            capturedUser.UserName.Should().Be("john.doe@example.com");
            capturedUser.PhoneNumber.Should().Be("+1234567890");
            capturedUser.BirthDate.Date.Should().Be(new DateTime(1995, 5, 15).Date);
            capturedUser.Gender.Should().Be(Gender.Male);
            capturedUser.CreatedAt.Date.Should().Be(DateTime.UtcNow.Date);
            result.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GenerateToken_ShouldCreateValidToken()
        {
            // Arrange
            var email = "test@example.com";
            var key = "test-secret-key";

            // Act
            var token = RegisterUserHandler.GenerateToken(email, key);

            // Assert
            token.Should().NotBeNullOrEmpty();
            // Since we can't easily validate the exact token (it contains timestamp)
            // we'll just verify it's a Base64 string with reasonable length
            token.Should().Match(t => IsBase64String(t));
            token.Length.Should().BeGreaterThan(20);
        }

        private bool IsBase64String(string base64)
        {
            try
            {
                var data = Convert.FromBase64String(base64);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
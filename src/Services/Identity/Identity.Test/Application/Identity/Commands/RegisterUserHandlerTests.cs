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
using MassTransit;
using MediatR;

namespace Identity.Test.Application.Identity.Commands
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IOptions<EndpointSettings>> _endpointSettingsMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly EndpointSettings _endpointSettings;

        public RegisterUserHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();

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
            var command = new RegisterUserCommand("First", "Last", "test@example.com", "+1234567890", DateTime.UtcNow.AddYears(-20), "Male", "Password123");

            // Setup userRepository to return null for GetUserByEmailAsync (email not taken)
            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync("test@example.com"))
                .ReturnsAsync((User)null);

            var handler = new RegisterUserHandler(
                _userRepositoryMock.Object,
                _endpointSettingsMock.Object,
                _publishEndpointMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            // Verify that publish was called for the email verification
            _publishEndpointMock.Verify(x => x.Publish(
                It.IsAny<BuildingBlocks.Messaging.Events.SendMailEvent>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var command = new RegisterUserCommand("First", "Last", "existing@example.com", "+1234567890", DateTime.UtcNow.AddYears(-20), "Male", "Password123");

            // Setup userRepository to return an existing user
            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync("existing@example.com"))
                .ReturnsAsync(new User { Email = "existing@example.com" });

            var handler = new RegisterUserHandler(
                _userRepositoryMock.Object,
                _endpointSettingsMock.Object,
                _publishEndpointMock.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Email already taken");
        }

        [Fact]
        public void GenerateTokenWithHashedPassword_ShouldCreateValidToken()
        {
            // Arrange
            var command = new RegisterUserCommand(
                "First",
                "Last",
                "test@example.com",
                "+1234567890",
                new DateTime(2000, 1, 1),
                "Male",
                "Password123");
            var key = "test-secret-key";

            // Act
            var token = RegisterUserHandler.GenerateTokenWithHashedPassword(command, key);

            // Assert
            token.Should().NotBeNullOrEmpty();
            // Since we can't easily validate the exact token (it contains hashed data)
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
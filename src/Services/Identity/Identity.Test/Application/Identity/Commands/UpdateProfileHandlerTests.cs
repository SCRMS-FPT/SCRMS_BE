using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Identity.Commands.UpdateProfile;
using Identity.Application.Dtos;
using Identity.Application.Services;
using Identity.Domain.Exceptions;
using Identity.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Identity.Application.Data.Repositories;

namespace Identity.Test.Application.Identity.Commands
{
    public class UpdateProfileHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IImageKitService> _imageKitServiceMock;

        public UpdateProfileHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _imageKitServiceMock = new Mock<IImageKitService>();
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
            _userRepositoryMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            var handler = new UpdateProfileHandler(_userRepositoryMock.Object, _imageKitServiceMock.Object);
            var command = new UpdateProfileCommand(
                user.Id,
                "NewFirst",
                "NewLast",
                "+987654321",
                DateTime.UtcNow.AddYears(-20),
                "Male",
                "Hello!"
            );

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
                .ReturnsAsync((User?)null);
            var handler = new UpdateProfileHandler(_userRepositoryMock.Object, _imageKitServiceMock.Object);
            var command = new UpdateProfileCommand(
                Guid.NewGuid(),
                "NewFirst",
                "NewLast",
                "+987654321",
                DateTime.UtcNow.AddYears(-20),
                "Male",
                "Hello!"
            );

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("User not found");
        }

        [Fact]
        public async Task Handle_ShouldUpdateImagesSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingImageUrls = new List<string> { "image1.jpg", "image2.jpg" };
            var user = new User
            {
                Id = userId,
                FirstName = "Old",
                LastName = "Name",
                PhoneNumber = "123",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                Gender = Gender.Male,
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            };

            // Add existing images to user
            user.SetImageUrlsList(existingImageUrls);

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdateUserAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            _userRepositoryMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Setup for image deletion
            _imageKitServiceMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Create a mock IFormFile for the avatar
            var mockAvatarFile = CreateMockFormFile("avatar.jpg", "image/jpeg");

            // Setup for new image uploads
            _imageKitServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("new-avatar.jpg");

            var handler = new UpdateProfileHandler(_userRepositoryMock.Object, _imageKitServiceMock.Object);
            var command = new UpdateProfileCommand(
                userId,
                "NewFirst",
                "NewLast",
                "+987654321",
                DateTime.UtcNow.AddYears(-20),
                "Male",
                "Hello!",
                mockAvatarFile,
                null,
                new List<string> { "image1.jpg" },
                new List<string> { "image2.jpg" }
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.FirstName.Should().Be("NewFirst");
            result.ImageUrls.Should().NotBeNull();
            result.ImageUrls.Should().Contain("new-avatar.jpg");
            result.ImageUrls.Should().Contain("image1.jpg");
            result.ImageUrls.Should().NotContain("image2.jpg");

            // Verify image deletion was called
            _imageKitServiceMock.Verify(x => x.DeleteFileAsync("image2.jpg", It.IsAny<CancellationToken>()), Times.Once);
        }

        // Helper method to create mock IFormFile
        private IFormFile CreateMockFormFile(string fileName, string contentType)
        {
            var content = "mock file content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.ContentType).Returns(contentType);
            formFile.Setup(f => f.Length).Returns(stream.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(stream);
            formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return formFile.Object;
        }
    }
}
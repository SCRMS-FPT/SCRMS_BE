using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Coaches.CreateCoach;
using Coach.API.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Coach.API.Tests.Coaches
{
    public class CreateCoachCommandHandlerTests
    {
        // Test 1: Tạo coach hợp lệ (Normal)
        [Fact]
        public async Task Handle_ValidCoach_CreatesCoachSuccessfully()
        {
            // Arrange
            var command = new CreateCoachCommand(
                UserId: Guid.NewGuid(),
                FullName: "Test Coach",
                Email: "coach@example.com",
                Phone: "1234567890",
                AvatarFile: null,
                ImageFiles: new List<IFormFile>(),
                Bio: "Experienced coach",
                RatePerHour: 50m,
                SportIds: new List<Guid> { Guid.NewGuid() }
            );

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.CoachExistsAsync(command.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            mockCoachRepo.Setup(r => r.AddCoachAsync(It.IsAny<Data.Models.Coach>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.AddCoachSportAsync(It.IsAny<CoachSport>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockImageKitService = new Mock<IImageKitService>();
            mockImageKitService.Setup(s => s.UploadFileAsync(
                    It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("http://example.com/avatar.jpg");

            mockImageKitService.Setup(s => s.UploadFilesAsync(
                    It.IsAny<List<IFormFile>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreateCoachCommandHandler(
                mockCoachRepo.Object,
                mockSportRepo.Object,
                mockImageKitService.Object,
                mockContext.Object
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockCoachRepo.Verify(r => r.AddCoachAsync(It.IsAny<Data.Models.Coach>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSportRepo.Verify(r => r.AddCoachSportAsync(It.IsAny<CoachSport>(), It.IsAny<CancellationToken>()), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(command.UserId, result.Id);
        }

        // Test 2: Coach đã tồn tại (Abnormal)
        [Fact]
        public async Task Handle_CoachAlreadyExists_ThrowsAlreadyExistsException()
        {
            // Arrange
            var command = new CreateCoachCommand(
                UserId: Guid.NewGuid(),
                FullName: "Test Coach",
                Email: "coach@example.com",
                Phone: "1234567890",
                AvatarFile: null,
                ImageFiles: new List<IFormFile>(),
                Bio: "Test",
                RatePerHour: 50m,
                SportIds: new List<Guid> { Guid.NewGuid() }
            );

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.CoachExistsAsync(command.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            var mockImageKitService = new Mock<IImageKitService>();
            var mockContext = new Mock<CoachDbContext>();

            var handler = new CreateCoachCommandHandler(
                mockCoachRepo.Object,
                mockSportRepo.Object,
                mockImageKitService.Object,
                mockContext.Object
            );

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AlreadyExistsException>(
                () => handler.Handle(command, CancellationToken.None)
            );
            Assert.Contains("was already exist", exception.Message);
        }

        // Test 3: Bio rỗng (Abnormal)
        [Fact]
        public void Validate_EmptyBio_ValidationFails()
        {
            // Arrange
            var command = new CreateCoachCommand(
                UserId: Guid.NewGuid(),
                FullName: "Test Coach",
                Email: "coach@example.com",
                Phone: "1234567890",
                AvatarFile: null,
                ImageFiles: new List<IFormFile>(),
                Bio: "",
                RatePerHour: 50m,
                SportIds: new List<Guid> { Guid.NewGuid() }
            );

            var validator = new CreateCoachCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Bio"));
        }

        // Test 4: RatePerHour âm hoặc bằng 0 (Abnormal)
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Validate_InvalidRatePerHour_ValidationFails(decimal ratePerHour)
        {
            // Arrange
            var command = new CreateCoachCommand(
                UserId: Guid.NewGuid(),
                FullName: "Test Coach",
                Email: "coach@example.com",
                Phone: "1234567890",
                AvatarFile: null,
                ImageFiles: new List<IFormFile>(),
                Bio: "Test",
                RatePerHour: ratePerHour,
                SportIds: new List<Guid> { Guid.NewGuid() }
            );

            var validator = new CreateCoachCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Rate"));
        }

        // Test 5: SportIds rỗng (Abnormal)
        [Fact]
        public void Validate_EmptySportIds_ValidationFails()
        {
            // Arrange
            var command = new CreateCoachCommand(
                UserId: Guid.NewGuid(),
                FullName: "Test Coach",
                Email: "coach@example.com",
                Phone: "1234567890",
                AvatarFile: null,
                ImageFiles: new List<IFormFile>(),
                Bio: "Test",
                RatePerHour: 50m,
                SportIds: new List<Guid>()
            );

            var validator = new CreateCoachCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("sport"));
        }

        // Test 6: SportIds chứa nhiều giá trị (Boundary)
        [Fact]
        public async Task Handle_MultipleSportIds_CreatesCoachSuccessfully()
        {
            // Arrange
            var sportIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var command = new CreateCoachCommand(
                UserId: Guid.NewGuid(),
                FullName: "Test Coach",
                Email: "coach@example.com",
                Phone: "1234567890",
                AvatarFile: null,
                ImageFiles: new List<IFormFile>(),
                Bio: "Test",
                RatePerHour: 50m,
                SportIds: sportIds
            );

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.CoachExistsAsync(command.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            mockCoachRepo.Setup(r => r.AddCoachAsync(It.IsAny<Data.Models.Coach>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.AddCoachSportAsync(It.IsAny<CoachSport>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockImageKitService = new Mock<IImageKitService>();
            mockImageKitService.Setup(s => s.UploadFilesAsync(
                    It.IsAny<List<IFormFile>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreateCoachCommandHandler(
                mockCoachRepo.Object,
                mockSportRepo.Object,
                mockImageKitService.Object,
                mockContext.Object
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockSportRepo.Verify(r => r.AddCoachSportAsync(It.IsAny<CoachSport>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Assert.Equal(command.UserId, result.Id);
        }
    }
}
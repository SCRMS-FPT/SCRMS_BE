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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Coach.API.Tests.Coaches
{
    public class CreateCoachHandlerTests
    {
        // Normal Case
        [Fact]
        public async Task Handle_ValidRequest_CreatesCoach()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var sportIds = new List<Guid> { sportId };
            
            var mockFormFile = new Mock<IFormFile>();
            var mockImageFiles = new List<IFormFile> { Mock.Of<IFormFile>() };
            
            var command = new CreateCoachCommand(
                userId,
                "John Doe",
                "john@example.com",
                "1234567890",
                mockFormFile.Object,
                mockImageFiles,
                "Professional coach",
                50m,
                sportIds
            );
            
            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.CoachExistsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockCoachRepo.Setup(r => r.AddCoachAsync(It.IsAny<Data.Models.Coach>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.AddCoachSportAsync(It.IsAny<CoachSport>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            var mockImageKitService = new Mock<IImageKitService>();
            mockImageKitService.Setup(s => s.UploadFileAsync(
                    It.IsAny<IFormFile>(), 
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("avatar-url.jpg");
            mockImageKitService.Setup(s => s.UploadFilesAsync(
                    It.IsAny<List<IFormFile>>(), 
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "image-url.jpg" });
            
            // Mock DbContext
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb" + Guid.NewGuid().ToString())
                .Options;
            var mockContext = new Mock<CoachDbContext>(options);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            
            var handler = new CreateCoachCommandHandler(
                mockCoachRepo.Object, 
                mockSportRepo.Object, 
                mockImageKitService.Object,
                mockContext.Object);
            
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("avatar-url.jpg", result.AvatarUrl);
            Assert.Contains("image-url.jpg", result.ImageUrls);
            Assert.Contains(sportId, result.SportIds);
            
            mockCoachRepo.Verify(r => r.AddCoachAsync(
                It.Is<Data.Models.Coach>(c => c.UserId == userId && c.FullName == "John Doe"), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
            
            mockSportRepo.Verify(r => r.AddCoachSportAsync(
                It.Is<CoachSport>(s => s.CoachId == userId && s.SportId == sportId), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        // Abnormal Case
        [Fact]
        public async Task Handle_CoachAlreadyExists_ThrowsAlreadyExistsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            var mockFormFile = new Mock<IFormFile>();
            var mockImageFiles = new List<IFormFile> { Mock.Of<IFormFile>() };
            
            var command = new CreateCoachCommand(
                userId,
                "John Doe",
                "john@example.com",
                "1234567890",
                mockFormFile.Object,
                mockImageFiles,
                "Professional coach",
                50m,
                new List<Guid> { Guid.NewGuid() }
            );
            
            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.CoachExistsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb" + Guid.NewGuid().ToString())
                .Options;
            var mockContext = new CoachDbContext(options);
            
            var handler = new CreateCoachCommandHandler(
                mockCoachRepo.Object, 
                Mock.Of<ICoachSportRepository>(), 
                Mock.Of<IImageKitService>(),
                mockContext);
            
            // Act & Assert
            await Assert.ThrowsAsync<AlreadyExistsException>(
                () => handler.Handle(command, CancellationToken.None));
        }
        
        // Boundary Case
        [Fact]
        public async Task Handle_EmptySportIds_ValidationFails()
        {
            // Arrange
            var command = new CreateCoachCommand(
                Guid.NewGuid(),
                "John Doe",
                "john@example.com",
                "1234567890",
                null,
                new List<IFormFile>(),
                "Professional coach",
                50m,
                new List<Guid>()  // Empty sport IDs
            );
            
            var validator = new CreateCoachCommandValidator();
            
            // Act
            var result = validator.Validate(command);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("sport required"));
        }
        
        [Fact]
        public async Task Handle_NegativeRate_ValidationFails()
        {
            // Arrange
            var command = new CreateCoachCommand(
                Guid.NewGuid(),
                "John Doe",
                "john@example.com",
                "1234567890",
                null,
                new List<IFormFile>(),
                "Professional coach",
                -10m,  // Negative rate
                new List<Guid> { Guid.NewGuid() }
            );
            
            var validator = new CreateCoachCommandValidator();
            
            // Act
            var result = validator.Validate(command);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "RatePerHour");
        }
    }
}
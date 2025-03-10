using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using Coach.API.Coaches.CreateCoach;
using Coach.API.Data;
using Coach.API.Data.Repositories;
using Coach.API.Models;
using FluentValidation;
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
            var command = new CreateCoachCommand(Guid.NewGuid(), "Experienced coach", 50m, new List<Guid> { Guid.NewGuid() });
            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.CoachExistsAsync(command.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            mockCoachRepo.Setup(r => r.AddCoachAsync(It.IsAny<Models.Coach>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.AddCoachSportAsync(It.IsAny<CoachSport>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreateCoachCommandHandler(mockCoachRepo.Object, mockSportRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockCoachRepo.Verify(r => r.AddCoachAsync(It.IsAny<Models.Coach>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSportRepo.Verify(r => r.AddCoachSportAsync(It.IsAny<CoachSport>(), It.IsAny<CancellationToken>()), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(command.UserId, result.Id);
        }

        // Test 2: Coach đã tồn tại (Abnormal)
        [Fact]
        public async Task Handle_CoachAlreadyExists_ThrowsAlreadyExistsException()
        {
            // Arrange
            var command = new CreateCoachCommand(Guid.NewGuid(), "Test", 50m, new List<Guid> { Guid.NewGuid() });
            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.CoachExistsAsync(command.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var handler = new CreateCoachCommandHandler(mockCoachRepo.Object, null, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AlreadyExistsException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal($"Entity \"Coach\" ({command.UserId}) was already exist.", exception.Message);
        }

        // Test 3: Bio rỗng (Abnormal)
        [Fact]
        public void Validate_EmptyBio_ValidationFails()
        {
            // Arrange
            var command = new CreateCoachCommand(Guid.NewGuid(), "", 50m, new List<Guid> { Guid.NewGuid() });
            var validator = new CreateCoachCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "'Bio' must not be empty.");
        }

        // Test 4: RatePerHour âm hoặc bằng 0 (Abnormal)
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Validate_InvalidRatePerHour_ValidationFails(decimal ratePerHour)
        {
            // Arrange
            var command = new CreateCoachCommand(Guid.NewGuid(), "Test", ratePerHour, new List<Guid> { Guid.NewGuid() });
            var validator = new CreateCoachCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "'Rate Per Hour' must be greater than '0'.");
        }

        // Test 5: SportIds rỗng (Abnormal)
        [Fact]
        public void Validate_EmptySportIds_ValidationFails()
        {
            // Arrange
            var command = new CreateCoachCommand(Guid.NewGuid(), "Test", 50m, new List<Guid>());
            var validator = new CreateCoachCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "At least one sport required");
        }

        // Test 6: SportIds chứa nhiều giá trị (Boundary)
        [Fact]
        public async Task Handle_MultipleSportIds_CreatesCoachSuccessfully()
        {
            // Arrange
            var sportIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var command = new CreateCoachCommand(Guid.NewGuid(), "Test", 50m, sportIds);
            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.CoachExistsAsync(command.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            mockCoachRepo.Setup(r => r.AddCoachAsync(It.IsAny<Models.Coach>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockSportRepo = new Mock<ICoachSportRepository>();
            mockSportRepo.Setup(r => r.AddCoachSportAsync(It.IsAny<CoachSport>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreateCoachCommandHandler(mockCoachRepo.Object, mockSportRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockSportRepo.Verify(r => r.AddCoachSportAsync(It.IsAny<CoachSport>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Assert.Equal(command.UserId, result.Id);
        }
    }
}
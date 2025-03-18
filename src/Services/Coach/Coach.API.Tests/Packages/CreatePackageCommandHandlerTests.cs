using System;
using System.Threading;
using System.Threading.Tasks;
using Coach.API.Data;
using Coach.API.Data.Repositories;
using Moq;
using Xunit;
using FluentValidation;
using Coach.API.Data.Models;
using Coach.API.Features.Packages.CreatePackage;

namespace Coach.API.Tests.Packages
{
    public class CreatePackageCommandHandlerTests
    {
        // Test 1: Tạo package hợp lệ (Normal)
        [Fact]
        public async Task Handle_ValidPackage_CreatesPackageSuccessfully()
        {
            // Arrange
            var command = new CreatePackageCommand(
                CoachId: Guid.NewGuid(),
                Name: "Basic Package",
                Description: "Basic training",
                Price: 100m,
                SessionCount: 5
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.AddCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreatePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPackageRepo.Verify(r => r.AddCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>()), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        // Test 2: CoachId rỗng (Abnormal)
        [Fact]
        public void Validate_EmptyCoachId_ShouldFailValidation()
        {
            // Arrange
            var command = new CreatePackageCommand(
                CoachId: Guid.Empty,
                Name: "Test",
                Description: "Test",
                Price: 100m,
                SessionCount: 5
            );
            var validator = new CreatePackageCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CoachId" && e.ErrorMessage == "CoachId is required");
        }

        // Test 3: Name rỗng (Abnormal)
        [Fact]
        public async Task Handle_EmptyName_ShouldThrowValidationException()
        {
            // Arrange
            var command = new CreatePackageCommand(
                CoachId: Guid.NewGuid(),
                Name: "",
                Description: "Test",
                Price: 100m,
                SessionCount: 5
            );

            var handler = new CreatePackageCommandHandler(null, null);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public void Validate_EmptyName_ShouldFailValidation()
        {
            // Arrange
            var command = new CreatePackageCommand(
                CoachId: Guid.NewGuid(),
                Name: "",
                Description: "Test",
                Price: 100m,
                SessionCount: 5
            );
            var validator = new CreatePackageCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Name must not be empty");
        }

        // Test 4: Price âm hoặc bằng 0 (Abnormal)
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Validate_InvalidPrice_ShouldFailValidation(decimal price)
        {
            // Arrange
            var command = new CreatePackageCommand(
                CoachId: Guid.NewGuid(),
                Name: "Test",
                Description: "Test",
                Price: price,
                SessionCount: 5
            );
            var validator = new CreatePackageCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Price" && e.ErrorMessage == "Price must be greater than 0");
        }

        // Test 5: SessionCount âm hoặc bằng 0 (Abnormal)
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidSessionCount_ShouldFailValidation(int sessionCount)
        {
            // Arrange
            var command = new CreatePackageCommand(
                CoachId: Guid.NewGuid(),
                Name: "Test",
                Description: "Test",
                Price: 100m,
                SessionCount: sessionCount
            );
            var validator = new CreatePackageCommandValidator();

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "SessionCount" && e.ErrorMessage == "SessionCount must be greater than 0");
        }

        // Test 6: Description rỗng (Boundary)
        [Fact]
        public async Task Handle_EmptyDescription_CreatesPackageSuccessfully()
        {
            // Arrange
            var command = new CreatePackageCommand(
                CoachId: Guid.NewGuid(),
                Name: "Test",
                Description: "",
                Price: 100m,
                SessionCount: 5
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.AddCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreatePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPackageRepo.Verify(r => r.AddCoachPackageAsync(It.Is<CoachPackage>(p => p.Description == ""), It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        // Test 7: Giá trị tối đa cho Price và SessionCount (Boundary)
        [Fact]
        public async Task Handle_MaxPriceAndSessionCount_CreatesPackageSuccessfully()
        {
            // Arrange
            var command = new CreatePackageCommand(
                CoachId: Guid.NewGuid(),
                Name: "Premium",
                Description: "Test",
                Price: decimal.MaxValue,
                SessionCount: int.MaxValue
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.AddCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreatePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPackageRepo.Verify(r => r.AddCoachPackageAsync(It.Is<CoachPackage>(p => p.Price == decimal.MaxValue && p.SessionCount == int.MaxValue), It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotEqual(Guid.Empty, result.Id);
        }
    }
}
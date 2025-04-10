using System;
using System.Threading;
using System.Threading.Tasks;
using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Packages.UpdatePackage;
using BuildingBlocks.Exceptions;
using Moq;
using Xunit;
using FluentValidation;
using FluentValidation.TestHelper;

namespace Coach.API.Tests.Packages
{
    public class UpdatePackageCommandHandlerTests
    {
        // Normal cases
        [Fact]
        public async Task Handle_ValidPackageUpdate_UpdatesPackageSuccessfully()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var coachId = Guid.NewGuid();

            var existingPackage = new CoachPackage
            {
                Id = packageId,
                CoachId = coachId,
                Name = "Original Name",
                Description = "Original Description",
                Price = 100m,
                SessionCount = 5,
                Status = "active"
            };

            var command = new UpdatePackageCommand(
                PackageId: packageId,
                CoachId: coachId,
                Name: "Updated Name",
                Description: "Updated Description",
                Price: 150m,
                SessionCount: 10,
                Status: "active"
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);
            mockPackageRepo.Setup(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new UpdatePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.Is<CoachPackage>(p =>
                p.Id == packageId &&
                p.Name == "Updated Name" &&
                p.Description == "Updated Description" &&
                p.Price == 150m &&
                p.SessionCount == 10 &&
                p.Status == "active"),
                It.IsAny<CancellationToken>()), Times.Once);

            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(packageId, result.Id);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(150m, result.Price);
            Assert.Equal(10, result.SessionCount);
            Assert.Equal("active", result.Status);
        }

        [Fact]
        public async Task Handle_ChangeStatusFromActiveToInactive_UpdatesPackageSuccessfully()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var coachId = Guid.NewGuid();

            var existingPackage = new CoachPackage
            {
                Id = packageId,
                CoachId = coachId,
                Name = "Test Package",
                Description = "Test Description",
                Price = 100m,
                SessionCount = 5,
                Status = "active"
            };

            var command = new UpdatePackageCommand(
                PackageId: packageId,
                CoachId: coachId,
                Name: "Test Package",
                Description: "Test Description",
                Price: 100m,
                SessionCount: 5,
                Status: "inactive"
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);
            mockPackageRepo.Setup(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new UpdatePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.Is<CoachPackage>(p =>
                p.Status == "inactive"),
                It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal("inactive", result.Status);
        }

        // Abnormal cases
        [Fact]
        public async Task Handle_PackageNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var command = new UpdatePackageCommand(
                PackageId: packageId,
                CoachId: Guid.NewGuid(),
                Name: "Test",
                Description: "Test",
                Price: 100m,
                SessionCount: 5,
                Status: "active"
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CoachPackage)null);

            var mockContext = new Mock<CoachDbContext>();
            var handler = new UpdatePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UnauthorizedCoachId_ThrowsUnauthorizedException()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var packageOwnerId = Guid.NewGuid();
            var differentCoachId = Guid.NewGuid();

            var existingPackage = new CoachPackage
            {
                Id = packageId,
                CoachId = packageOwnerId,  // Original coach
                Name = "Test Package",
                Description = "Test Description",
                Price = 100m,
                SessionCount = 5
            };

            var command = new UpdatePackageCommand(
                PackageId: packageId,
                CoachId: differentCoachId,  // Different coach trying to update
                Name: "Updated Package",
                Description: "Updated Description",
                Price: 150m,
                SessionCount: 10,
                Status: "active"
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);

            var mockContext = new Mock<CoachDbContext>();
            var handler = new UpdatePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(command, CancellationToken.None));

            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        // Validation tests
        [Fact]
        public void Validate_EmptyPackageId_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdatePackageCommand(
                PackageId: Guid.Empty,
                CoachId: Guid.NewGuid(),
                Name: "Test",
                Description: "Test",
                Price: 100m,
                SessionCount: 5,
                Status: "active"
            );
            var validator = new UpdatePackageCommandValidator();

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PackageId)
                .WithErrorMessage("PackageId is required");
        }

        [Fact]
        public void Validate_EmptyCoachId_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdatePackageCommand(
                PackageId: Guid.NewGuid(),
                CoachId: Guid.Empty,
                Name: "Test",
                Description: "Test",
                Price: 100m,
                SessionCount: 5,
                Status: "active"
            );
            var validator = new UpdatePackageCommandValidator();

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CoachId)
                .WithErrorMessage("CoachId is required");
        }

        [Fact]
        public void Validate_EmptyName_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdatePackageCommand(
                PackageId: Guid.NewGuid(),
                CoachId: Guid.NewGuid(),
                Name: "",
                Description: "Test",
                Price: 100m,
                SessionCount: 5,
                Status: "active"
            );
            var validator = new UpdatePackageCommandValidator();

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Name must not be empty");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPrice_ShouldFailValidation(decimal price)
        {
            // Arrange
            var command = new UpdatePackageCommand(
                PackageId: Guid.NewGuid(),
                CoachId: Guid.NewGuid(),
                Name: "Test",
                Description: "Test",
                Price: price,
                SessionCount: 5,
                Status: "active"
            );
            var validator = new UpdatePackageCommandValidator();

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Price)
                .WithErrorMessage("Price must be greater than 0");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidSessionCount_ShouldFailValidation(int sessionCount)
        {
            // Arrange
            var command = new UpdatePackageCommand(
                PackageId: Guid.NewGuid(),
                CoachId: Guid.NewGuid(),
                Name: "Test",
                Description: "Test",
                Price: 100m,
                SessionCount: sessionCount,
                Status: "active"
            );
            var validator = new UpdatePackageCommandValidator();

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SessionCount)
                .WithErrorMessage("SessionCount must be greater than 0");
        }

        [Theory]
        [InlineData("pending")]
        [InlineData("cancelled")]
        [InlineData("")]
        public void Validate_InvalidStatus_ShouldFailValidation(string status)
        {
            // Arrange
            var command = new UpdatePackageCommand(
                PackageId: Guid.NewGuid(),
                CoachId: Guid.NewGuid(),
                Name: "Test",
                Description: "Test",
                Price: 100m,
                SessionCount: 5,
                Status: status
            );
            var validator = new UpdatePackageCommandValidator();

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage("Status must be either 'active' or 'inactive'");
        }

        // Boundary cases
        [Fact]
        public async Task Handle_EmptyDescription_UpdatesPackageSuccessfully()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var coachId = Guid.NewGuid();

            var existingPackage = new CoachPackage
            {
                Id = packageId,
                CoachId = coachId,
                Name = "Test Package",
                Description = "Original Description",
                Price = 100m,
                SessionCount = 5,
                Status = "active"
            };

            var command = new UpdatePackageCommand(
                PackageId: packageId,
                CoachId: coachId,
                Name: "Test Package",
                Description: "",  // Empty description
                Price: 100m,
                SessionCount: 5,
                Status: "active"
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);
            mockPackageRepo.Setup(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new UpdatePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.Is<CoachPackage>(p =>
                p.Description == ""),
                It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal("", result.Description);
        }

        [Fact]
        public async Task Handle_MaxPriceAndSessionCount_UpdatesPackageSuccessfully()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var coachId = Guid.NewGuid();

            var existingPackage = new CoachPackage
            {
                Id = packageId,
                CoachId = coachId,
                Name = "Test Package",
                Description = "Test Description",
                Price = 100m,
                SessionCount = 5,
                Status = "active"
            };

            var command = new UpdatePackageCommand(
                PackageId: packageId,
                CoachId: coachId,
                Name: "Test Package",
                Description: "Test Description",
                Price: decimal.MaxValue,  // Max price
                SessionCount: int.MaxValue,  // Max sessions
                Status: "active"
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);
            mockPackageRepo.Setup(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new UpdatePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.Is<CoachPackage>(p =>
                p.Price == decimal.MaxValue &&
                p.SessionCount == int.MaxValue),
                It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(decimal.MaxValue, result.Price);
            Assert.Equal(int.MaxValue, result.SessionCount);
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Packages.DeletePackage;
using BuildingBlocks.Exceptions;
using Moq;
using Xunit;
using FluentValidation.TestHelper;

namespace Coach.API.Tests.Packages
{
    public class DeletePackageCommandHandlerTests
    {
        // Normal cases
        [Fact]
        public async Task Handle_ValidDelete_DeactivatesPackageSuccessfully()
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

            var command = new DeletePackageCommand(
                PackageId: packageId,
                CoachId: coachId
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);
            mockPackageRepo.Setup(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new DeletePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.Is<CoachPackage>(p =>
                p.Id == packageId &&
                p.Status == "inactive"),
                It.IsAny<CancellationToken>()), Times.Once);

            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(packageId, result.Id);
            Assert.Equal("inactive", result.Status);
            Assert.Equal("Package successfully deactivated", result.Message);
        }

        [Fact]
        public async Task Handle_AlreadyInactivePackage_StillReturnsSuccess()
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
                Status = "inactive"  // Already inactive
            };

            var command = new DeletePackageCommand(
                PackageId: packageId,
                CoachId: coachId
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);
            mockPackageRepo.Setup(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new DeletePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(),
                It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(packageId, result.Id);
            Assert.Equal("inactive", result.Status);
        }

        // Abnormal cases
        [Fact]
        public async Task Handle_PackageNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var coachId = Guid.NewGuid();

            var command = new DeletePackageCommand(
                PackageId: packageId,
                CoachId: coachId
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CoachPackage)null);

            var mockContext = new Mock<CoachDbContext>();

            var handler = new DeletePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UnauthorizedCoach_ThrowsUnauthorizedException()
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
                SessionCount = 5,
                Status = "active"
            };

            var command = new DeletePackageCommand(
                PackageId: packageId,
                CoachId: differentCoachId  // Different coach trying to delete
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);

            var mockContext = new Mock<CoachDbContext>();

            var handler = new DeletePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(command, CancellationToken.None));

            mockPackageRepo.Verify(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        // Boundary cases
        [Fact]
        public async Task Handle_EmptyDescription_DeactivatesPackageSuccessfully()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var coachId = Guid.NewGuid();

            var existingPackage = new CoachPackage
            {
                Id = packageId,
                CoachId = coachId,
                Name = "Test Package",
                Description = "",  // Empty description
                Price = 100m,
                SessionCount = 5,
                Status = "active"
            };

            var command = new DeletePackageCommand(
                PackageId: packageId,
                CoachId: coachId
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);
            mockPackageRepo.Setup(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new DeletePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(packageId, result.Id);
            Assert.Equal("inactive", result.Status);
        }

        [Fact]
        public async Task Handle_ZeroPriceAndSessionCount_DeactivatesPackageSuccessfully()
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
                Price = 0,  // Zero price
                SessionCount = 0,  // Zero sessions
                Status = "active"
            };

            var command = new DeletePackageCommand(
                PackageId: packageId,
                CoachId: coachId
            );

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPackage);
            mockPackageRepo.Setup(r => r.UpdateCoachPackageAsync(It.IsAny<CoachPackage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new DeletePackageCommandHandler(mockPackageRepo.Object, mockContext.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(packageId, result.Id);
            Assert.Equal("inactive", result.Status);
        }
    }
}
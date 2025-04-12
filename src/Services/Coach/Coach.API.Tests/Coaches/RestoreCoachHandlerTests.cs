using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Coaches.RestoreCoach;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Coach.API.Tests.Coaches
{
    public class RestoreCoachHandlerTests
    {
        // Normal cases
        [Fact]
        public async Task Handle_DeletedCoach_RestoresSuccessfully()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            var coach = new Data.Models.Coach
            {
                UserId = coachId,
                FullName = "Test Coach",
                Email = "test@example.com",
                Status = "deleted"
            };

            // Create a queryable list that can be used as a mock DbSet
            var coaches = new List<Data.Models.Coach> { coach }.AsQueryable();
            var mockCoaches = coaches.BuildMockDbSet();

            var mockCoachRepository = new Mock<ICoachRepository>();
            mockCoachRepository
                .Setup(r => r.SetCoachStatusAsync(coachId, "active", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.Coaches).Returns(mockCoaches.Object);

            var handler = new RestoreCoachHandler(mockCoachRepository.Object, mockContext.Object);
            var command = new RestoreCoachCommand(coachId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            mockCoachRepository.Verify(
                r => r.SetCoachStatusAsync(coachId, "active", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // Abnormal cases
        [Fact]
        public async Task Handle_CoachNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            // Create an empty queryable list
            var coaches = new List<Data.Models.Coach>().AsQueryable();
            var mockCoaches = coaches.BuildMockDbSet();

            var mockCoachRepository = new Mock<ICoachRepository>();
            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.Coaches).Returns(mockCoaches.Object);

            var handler = new RestoreCoachHandler(mockCoachRepository.Object, mockContext.Object);
            var command = new RestoreCoachCommand(coachId);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(command, CancellationToken.None));

            mockCoachRepository.Verify(
                r => r.SetCoachStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_CoachNotInDeletedState_ThrowsBadRequestException()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            var coach = new Data.Models.Coach
            {
                UserId = coachId,
                FullName = "Test Coach",
                Email = "test@example.com",
                Status = "active" // Not in deleted state
            };

            // Create a queryable list for mock
            var coaches = new List<Data.Models.Coach> { coach }.AsQueryable();
            var mockCoaches = coaches.BuildMockDbSet();

            var mockCoachRepository = new Mock<ICoachRepository>();
            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.Coaches).Returns(mockCoaches.Object);

            var handler = new RestoreCoachHandler(mockCoachRepository.Object, mockContext.Object);
            var command = new RestoreCoachCommand(coachId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => handler.Handle(command, CancellationToken.None));

            Assert.Equal("Coach is not in deleted state", exception.Message);

            mockCoachRepository.Verify(
                r => r.SetCoachStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // Boundary cases
        [Theory]
        [InlineData("inactive")]
        [InlineData("pending")]
        public async Task Handle_CoachInOtherNonDeletedStates_ThrowsBadRequestException(string status)
        {
            // Arrange
            var coachId = Guid.NewGuid();

            var coach = new Data.Models.Coach
            {
                UserId = coachId,
                FullName = "Test Coach",
                Email = "test@example.com",
                Status = status // In a state other than "deleted"
            };

            // Create a queryable list for mock
            var coaches = new List<Data.Models.Coach> { coach }.AsQueryable();
            var mockCoaches = coaches.BuildMockDbSet();

            var mockCoachRepository = new Mock<ICoachRepository>();
            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.Coaches).Returns(mockCoaches.Object);

            var handler = new RestoreCoachHandler(mockCoachRepository.Object, mockContext.Object);
            var command = new RestoreCoachCommand(coachId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                () => handler.Handle(command, CancellationToken.None));

            Assert.Equal("Coach is not in deleted state", exception.Message);
        }

        [Fact]
        public async Task Handle_EmptyGuidCoachId_ThrowsArgumentException()
        {
            // Arrange
            var coachId = Guid.Empty;

            // Create an empty queryable list
            var coaches = new List<Data.Models.Coach>().AsQueryable();
            var mockCoaches = coaches.BuildMockDbSet();

            var mockCoachRepository = new Mock<ICoachRepository>();
            var mockContext = new Mock<CoachDbContext>();
            mockContext.Setup(c => c.Coaches).Returns(mockCoaches.Object);

            var handler = new RestoreCoachHandler(mockCoachRepository.Object, mockContext.Object);
            var command = new RestoreCoachCommand(coachId);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(command, CancellationToken.None));
        }
    }
}
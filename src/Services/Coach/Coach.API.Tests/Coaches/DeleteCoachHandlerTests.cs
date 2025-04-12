using System;
using System.Threading;
using System.Threading.Tasks;
using Coach.API.Data.Repositories;
using Coach.API.Features.Coaches.DeleteCoach;
using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Coach.API.Tests.Coaches
{
    public class DeleteCoachHandlerTests
    {
        // Normal cases
        [Fact]
        public async Task Handle_ExistingCoach_DeletesSuccessfully()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            
            var mockCoachRepository = new Mock<ICoachRepository>();
            mockCoachRepository.Setup(r => r.CoachExistsAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockCoachRepository.Setup(r => r.SetCoachStatusAsync(coachId, "deleted", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<DeleteCoachHandler>>();
            
            var handler = new DeleteCoachHandler(mockCoachRepository.Object, mockLogger.Object);
            var command = new DeleteCoachCommand(coachId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            mockCoachRepository.Verify(
                r => r.SetCoachStatusAsync(coachId, "deleted", It.IsAny<CancellationToken>()),
                Times.Once);
            mockCoachRepository.Verify(
                r => r.CoachExistsAsync(coachId, It.IsAny<CancellationToken>()),
                Times.Once);
            // Verify that logging occurred (optional)
            // This varies based on your logging implementation, but this is an example
            Moq.It.IsAny<ILogger<DeleteCoachHandler>>();
        }

        // Abnormal cases
        [Fact]
        public async Task Handle_CoachNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            
            var mockCoachRepository = new Mock<ICoachRepository>();
            mockCoachRepository.Setup(r => r.CoachExistsAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var mockLogger = new Mock<ILogger<DeleteCoachHandler>>();
            
            var handler = new DeleteCoachHandler(mockCoachRepository.Object, mockLogger.Object);
            var command = new DeleteCoachCommand(coachId);

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

        // Boundary cases
        [Fact]
        public async Task Handle_EmptyGuidCoachId_ThrowsNotFoundException()
        {
            // Arrange
            var coachId = Guid.Empty;
            
            var mockCoachRepository = new Mock<ICoachRepository>();
            mockCoachRepository.Setup(r => r.CoachExistsAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // Assuming your repository correctly returns false for empty GUIDs
            
            var mockLogger = new Mock<ILogger<DeleteCoachHandler>>();
            
            var handler = new DeleteCoachHandler(mockCoachRepository.Object, mockLogger.Object);
            var command = new DeleteCoachCommand(coachId);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(command, CancellationToken.None));
        }
        
        [Fact]
        public async Task Handle_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            
            var mockCoachRepository = new Mock<ICoachRepository>();
            mockCoachRepository.Setup(r => r.CoachExistsAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockCoachRepository.Setup(r => r.SetCoachStatusAsync(coachId, "deleted", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));
            
            var mockLogger = new Mock<ILogger<DeleteCoachHandler>>();
            
            var handler = new DeleteCoachHandler(mockCoachRepository.Object, mockLogger.Object);
            var command = new DeleteCoachCommand(coachId);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));
        }
    }
}
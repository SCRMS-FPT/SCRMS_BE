using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.SportManagement.Commands.UpdateSport;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Commands
{
    public class UpdateSportHandlerTests
    {
        private readonly Mock<ISportRepository> _mockSportRepository;
        private readonly Mock<IValidator<UpdateSportCommand>> _mockValidator;
        private readonly UpdateSportHandler _handler;

        public UpdateSportHandlerTests()
        {
            _mockSportRepository = new Mock<ISportRepository>();
            _mockValidator = new Mock<IValidator<UpdateSportCommand>>();

            _handler = new UpdateSportHandler(_mockSportRepository.Object);

            // Default setup for validator
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateSportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        [Fact]
        public async Task Handle_Should_ThrowKeyNotFoundException_When_SportNotFound()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var command = new UpdateSportCommand(sportId, "Tennis", "Tennis is a racket sport", "tennis.png");

            _mockSportRepository.Setup(r => r.GetSportByIdAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sport)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            // Verify repository calls
            _mockSportRepository.Verify(r => r.GetSportByIdAsync(SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
            _mockSportRepository.Verify(r => r.UpdateSportAsync(It.IsAny<Sport>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_UpdateSport_When_SportExists()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var originalName = "Original Name";
            var originalDescription = "Original Description";
            var originalIcon = "original.png";

            var command = new UpdateSportCommand(sportId, "Tennis", "Tennis is a racket sport", "tennis.png");

            var sport = Sport.Create(
                SportId.Of(sportId),
                originalName,
                originalDescription,
                originalIcon
            );

            _mockSportRepository.Setup(r => r.GetSportByIdAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sport);

            _mockSportRepository.Setup(r => r.UpdateSportAsync(It.IsAny<Sport>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            // Verify repository calls
            _mockSportRepository.Verify(r => r.GetSportByIdAsync(SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
            _mockSportRepository.Verify(r => r.UpdateSportAsync(It.IsAny<Sport>(), It.IsAny<CancellationToken>()), Times.Once);

            // Verify sport properties were updated
            Assert.Equal("Tennis", sport.Name);
            Assert.Equal("Tennis is a racket sport", sport.Description);
            Assert.Equal("tennis.png", sport.Icon);
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessResult_When_SportIsUpdated()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var command = new UpdateSportCommand(sportId, "Tennis", "Tennis is a racket sport", "tennis.png");

            var sport = Sport.Create(
                SportId.Of(sportId),
                "Original Name",
                "Original Description",
                "original.png"
            );

            _mockSportRepository.Setup(r => r.GetSportByIdAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sport);

            _mockSportRepository.Setup(r => r.UpdateSportAsync(It.IsAny<Sport>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
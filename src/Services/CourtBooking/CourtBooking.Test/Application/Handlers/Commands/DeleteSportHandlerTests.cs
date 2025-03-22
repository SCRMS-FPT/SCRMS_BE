using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.SportManagement.Commands.DeleteSport;
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
    public class DeleteSportHandlerTests
    {
        private readonly Mock<ISportRepository> _mockSportRepository;
        private readonly Mock<IValidator<DeleteSportCommand>> _mockValidator;
        private readonly DeleteSportHandler _handler;

        public DeleteSportHandlerTests()
        {
            _mockSportRepository = new Mock<ISportRepository>();
            _mockValidator = new Mock<IValidator<DeleteSportCommand>>();

            _handler = new DeleteSportHandler(_mockSportRepository.Object);

            // Default setup for validator
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<DeleteSportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        [Fact]
        public async Task Handle_Should_ReturnFalse_When_SportNotFound()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var command = new DeleteSportCommand(sportId);

            _mockSportRepository.Setup(r => r.GetSportByIdAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sport)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.Message);

            // Verify repository calls
            _mockSportRepository.Verify(r => r.GetSportByIdAsync(SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
            _mockSportRepository.Verify(r => r.DeleteSportAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnFalse_When_SportIsInUse()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var command = new DeleteSportCommand(sportId);

            var sport = Sport.Create(
                SportId.Of(sportId),
                "Tennis",
                "Tennis is a racket sport",
                "tennis.png"
            );

            _mockSportRepository.Setup(r => r.GetSportByIdAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sport);

            _mockSportRepository.Setup(r => r.IsSportInUseAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("associated with", result.Message);

            // Verify repository calls
            _mockSportRepository.Verify(r => r.GetSportByIdAsync(SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
            _mockSportRepository.Verify(r => r.IsSportInUseAsync(SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
            _mockSportRepository.Verify(r => r.DeleteSportAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_DeleteSport_When_SportExistsAndNotInUse()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var command = new DeleteSportCommand(sportId);

            var sport = Sport.Create(
                SportId.Of(sportId),
                "Tennis",
                "Tennis is a racket sport",
                "tennis.png"
            );

            _mockSportRepository.Setup(r => r.GetSportByIdAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sport);

            _mockSportRepository.Setup(r => r.IsSportInUseAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockSportRepository.Setup(r => r.DeleteSportAsync(It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("successfully", result.Message);

            // Verify repository calls
            _mockSportRepository.Verify(r => r.GetSportByIdAsync(SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
            _mockSportRepository.Verify(r => r.IsSportInUseAsync(SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
            _mockSportRepository.Verify(r => r.DeleteSportAsync(SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
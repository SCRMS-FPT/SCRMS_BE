using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.SportManagement.Commands.CreateSport;
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
    public class CreateSportHandlerTests
    {
        private readonly Mock<ISportRepository> _mockSportRepository;
        private readonly Mock<IValidator<CreateSportCommand>> _mockValidator;
        private readonly CreateSportHandler _handler;

        public CreateSportHandlerTests()
        {
            _mockSportRepository = new Mock<ISportRepository>();
            _mockValidator = new Mock<IValidator<CreateSportCommand>>();

            _handler = new CreateSportHandler(_mockSportRepository.Object);

            // Default setup for validator
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateSportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        [Fact]
        public async Task Handle_Should_CreateSport_When_Called()
        {
            // Arrange
            var command = new CreateSportCommand("Tennis", "Tennis is a racket sport", "tennis.png");

            Sport addedSport = null;
            _mockSportRepository.Setup(r => r.AddSportAsync(It.IsAny<Sport>(), It.IsAny<CancellationToken>()))
                .Callback<Sport, CancellationToken>((s, _) => addedSport = s)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);

            // Verify repository calls
            _mockSportRepository.Verify(r => r.AddSportAsync(It.IsAny<Sport>(), It.IsAny<CancellationToken>()), Times.Once);

            // Verify sport properties
            Assert.NotNull(addedSport);
            Assert.Equal("Tennis", addedSport.Name);
            Assert.Equal("Tennis is a racket sport", addedSport.Description);
            Assert.Equal("tennis.png", addedSport.Icon);
        }

        [Fact]
        public async Task Handle_Should_Return_CreatedSportId_When_SportIsCreated()
        {
            // Arrange
            var command = new CreateSportCommand("Tennis", "Tennis is a racket sport", "tennis.png");

            _mockSportRepository.Setup(r => r.AddSportAsync(It.IsAny<Sport>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
        }
    }
}
using BuildingBlocks.Exceptions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.SportManagement.Queries.GetSportById;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetSportByIdHandlerTests
    {
        private readonly Mock<ISportRepository> _mockSportRepository;
        private readonly GetSportByIdHandler _handler;

        public GetSportByIdHandlerTests()
        {
            _mockSportRepository = new Mock<ISportRepository>();
            _handler = new GetSportByIdHandler(_mockSportRepository.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnNull_When_SportNotFound()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var query = new GetSportByIdQuery(sportId);

            _mockSportRepository.Setup(r => r.GetByIdAsync(
                    It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sport)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);

            _mockSportRepository.Verify(r => r.GetByIdAsync(
                SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnSportDTO_When_SportExists()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var query = new GetSportByIdQuery(sportId);

            var sport = Sport.Create(
                SportId.Of(sportId),
                "Tennis",
                "Tennis is a racket sport",
                "tennis.png"
            );

            _mockSportRepository.Setup(r => r.GetByIdAsync(
                    It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sport);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sportId, result.Id);
            Assert.Equal("Tennis", result.Name);
            Assert.Equal("tennis.png", result.Icon);
            Assert.Equal("Tennis is a racket sport", result.Description);

            _mockSportRepository.Verify(r => r.GetByIdAsync(
                SportId.Of(sportId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnCorrectDTO_When_SportHasEmptyDescription()
        {
            // Arrange - boundary test case for null/empty description
            var sportId = Guid.NewGuid();
            var query = new GetSportByIdQuery(sportId);

            var sport = Sport.Create(
                SportId.Of(sportId),
                "Tennis",
                "", // Empty description
                "tennis.png" // Icon
            );

            _mockSportRepository.Setup(r => r.GetByIdAsync(
                    It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sport);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sportId, result.Id);
            Assert.Equal("Tennis", result.Name);
            Assert.Equal("tennis.png", result.Icon);
            Assert.Equal("", result.Description);
        }

        [Fact]
        public async Task Handle_Should_CallRepositoryWithCorrectParameters()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var query = new GetSportByIdQuery(sportId);

            _mockSportRepository.Setup(r => r.GetByIdAsync(
                    It.IsAny<SportId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sport)null);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert - verify correct parameters
            _mockSportRepository.Verify(r => r.GetByIdAsync(
                It.Is<SportId>(id => id.Value == sportId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.SportManagement.Queries.GetSports;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetSportsHandlerTests
    {
        private readonly Mock<ISportRepository> _mockSportRepository;
        private readonly GetSportsHandler _handler;

        public GetSportsHandlerTests()
        {
            _mockSportRepository = new Mock<ISportRepository>();
            _handler = new GetSportsHandler(_mockSportRepository.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_When_NoSportsExist()
        {
            // Arrange
            var query = new GetSportsQuery();

            _mockSportRepository.Setup(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sport>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Sports);
            Assert.Empty(result.Sports);

            _mockSportRepository.Verify(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnSportsList_When_SportsExist()
        {
            // Arrange
            var query = new GetSportsQuery();
            var sportId1 = Guid.NewGuid();
            var sportId2 = Guid.NewGuid();

            var sports = new List<Sport>
            {
                Sport.Create(
                    SportId.Of(sportId1),
                    "Tennis",
                    "Tennis is a racket sport",
                    "tennis.png"
                ),
                Sport.Create(
                    SportId.Of(sportId2),
                    "Football",
                    "Football is a team sport",
                    "football.png"
                )
            };

            _mockSportRepository.Setup(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sports);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Sports);
            Assert.Equal(2, result.Sports.Count);

            // Verify first sport
            Assert.Equal(sportId1, result.Sports[0].Id);
            Assert.Equal("Tennis", result.Sports[0].Name);
            Assert.Equal("Tennis is a racket sport", result.Sports[0].Description);
            Assert.Equal("tennis.png", result.Sports[0].Icon);

            // Verify second sport
            Assert.Equal(sportId2, result.Sports[1].Id);
            Assert.Equal("Football", result.Sports[1].Name);
            Assert.Equal("Football is a team sport", result.Sports[1].Description);
            Assert.Equal("football.png", result.Sports[1].Icon);

            _mockSportRepository.Verify(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_HandleEmptyDescriptions_When_SportsExist()
        {
            // Arrange - boundary test case for null/empty descriptions
            var query = new GetSportsQuery();
            var sportId = Guid.NewGuid();

            var sports = new List<Sport>
            {
                Sport.Create(
                    SportId.Of(sportId),
                    "Tennis",
                    "", // Empty description
                    "tennis.png"
                )
            };

            _mockSportRepository.Setup(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sports);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Sports);
            Assert.Single(result.Sports);

            // Verify sport with empty description
            Assert.Equal(sportId, result.Sports[0].Id);
            Assert.Equal("Tennis", result.Sports[0].Name);
            Assert.Equal("", result.Sports[0].Description);
            Assert.Equal("tennis.png", result.Sports[0].Icon);
        }

        [Fact]
        public async Task Handle_Should_MapEachSportCorrectly_When_MultipleSportsExist()
        {
            // Arrange
            var query = new GetSportsQuery();

            // Create a large number of sports to test mapping correctness
            var sportsCount = 10;
            var sports = new List<Sport>();

            for (int i = 1; i <= sportsCount; i++)
            {
                sports.Add(Sport.Create(
                    SportId.Of(Guid.NewGuid()),
                    $"Sport {i}",
                    $"Description {i}",
                    $"icon{i}.png"
                ));
            }

            _mockSportRepository.Setup(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sports);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Sports);
            Assert.Equal(sportsCount, result.Sports.Count);

            // Verify all sports are mapped correctly
            for (int i = 0; i < sportsCount; i++)
            {
                Assert.Equal(sports[i].Id.Value, result.Sports[i].Id);
                Assert.Equal(sports[i].Name, result.Sports[i].Name);
                Assert.Equal(sports[i].Description, result.Sports[i].Description);
                Assert.Equal(sports[i].Icon, result.Sports[i].Icon);
            }
        }
    }
}
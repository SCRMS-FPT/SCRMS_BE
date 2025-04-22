using CourtBooking.Application.CourtManagement.Queries.GetAllCourtsOfSportCenter;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetAllCourtsOfSportCenterHandlerTests
    {
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly Mock<ISportRepository> _mockSportRepository;
        private readonly Mock<ICourtPromotionRepository> _mockPromotionRepository;
        private readonly GetAllCourtsOfSportCenterHandler _handler;

        public GetAllCourtsOfSportCenterHandlerTests()
        {
            _mockCourtRepository = new Mock<ICourtRepository>();
            _mockSportRepository = new Mock<ISportRepository>();
            _mockPromotionRepository = new Mock<ICourtPromotionRepository>();
            _handler = new GetAllCourtsOfSportCenterHandler(
                _mockCourtRepository.Object,
                _mockSportRepository.Object,
                _mockPromotionRepository.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_When_NoCourtExists()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var query = new GetAllCourtsOfSportCenterQuery(sportCenterId);

            _mockCourtRepository.Setup(r => r.GetAllCourtsOfSportCenterAsync(
                    It.IsAny<SportCenterId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Court>());

            _mockSportRepository.Setup(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sport>());

            _mockPromotionRepository.Setup(r => r.GetPromotionsByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result.Courts);

            _mockCourtRepository.Verify(r => r.GetAllCourtsOfSportCenterAsync(
                SportCenterId.Of(sportCenterId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnCourtsList_When_CourtsExist()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var query = new GetAllCourtsOfSportCenterQuery(sportCenterId);

            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(sportId),
                TimeSpan.FromMinutes(100),
                "Main court",
                "[]",
               (CourtType)1,
                30, 24,
                1
            );

            var sport = Sport.Create(
                SportId.Of(sportId),
                "Tennis",
                "Môn thể thao tennis",
                "tennis.jpg"
            );

            _mockCourtRepository.Setup(r => r.GetAllCourtsOfSportCenterAsync(
                    It.IsAny<SportCenterId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Court> { court });

            _mockSportRepository.Setup(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sport> { sport });

            _mockPromotionRepository.Setup(r => r.GetPromotionsByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Courts);
            var courtDto = result.Courts.First();
            Assert.Equal(courtId, courtDto.Id);
            Assert.Equal("Tennis Court 1", courtDto.CourtName);
            Assert.Equal(sportId, courtDto.SportId);
            Assert.Equal(sportCenterId, courtDto.SportCenterId);
            Assert.Equal("Tennis", courtDto.SportName);

            _mockCourtRepository.Verify(r => r.GetAllCourtsOfSportCenterAsync(
                SportCenterId.Of(sportCenterId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_SetUnknownSport_When_SportNotFound()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var query = new GetAllCourtsOfSportCenterQuery(sportCenterId);

            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(sportId),
                TimeSpan.FromMinutes(100),
                "Main court",
                "[]",
               (CourtType)1,
                30, 24,
                1
            );

            _mockCourtRepository.Setup(r => r.GetAllCourtsOfSportCenterAsync(
                    It.IsAny<SportCenterId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Court> { court });

            _mockSportRepository.Setup(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sport>()); // Empty sports list

            _mockPromotionRepository.Setup(r => r.GetPromotionsByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Courts);
            var courtDto = result.Courts.First();
            Assert.Equal("Unknown Sport", courtDto.SportName);
        }

        [Fact]
        public async Task Handle_Should_DeserializeFacilities_When_FacilitiesExist()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var query = new GetAllCourtsOfSportCenterQuery(sportCenterId);

            var facilities = new List<FacilityDTO>
            {
                new FacilityDTO { Name = "Shower", Description = "true" },
                new FacilityDTO { Name = "Locker", Description = "true" }
            };

            var facilitiesJson = JsonSerializer.Serialize(facilities);

            // Create the court with the facilities JSON directly in the constructor
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(sportId),
                TimeSpan.FromMinutes(100),
                "Main court",
                facilitiesJson, // Use the JSON here instead of "[]"
                (CourtType)1,
                30, 24,
                1
            );

            var sport = Sport.Create(
                SportId.Of(sportId),
                "Tennis",
                "Môn thể thao tennis",
                "tennis.jpg"
            );

            _mockCourtRepository.Setup(r => r.GetAllCourtsOfSportCenterAsync(
                    It.IsAny<SportCenterId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Court> { court });

            _mockSportRepository.Setup(r => r.GetAllSportsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sport> { sport });

            _mockPromotionRepository.Setup(r => r.GetPromotionsByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Courts);
            var courtDto = result.Courts.First();
            Assert.NotNull(courtDto.Facilities);
            Assert.Equal(2, courtDto.Facilities.Count);
            Assert.Equal("Shower", courtDto.Facilities[0].Name);
            Assert.Equal("Locker", courtDto.Facilities[1].Name);
        }
    }
}
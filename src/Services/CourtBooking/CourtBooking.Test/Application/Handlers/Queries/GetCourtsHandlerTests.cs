using CourtBooking.Application.CourtManagement.Queries.GetCourts;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BuildingBlocks.Pagination;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetCourtsHandlerTests
    {
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly Mock<ISportRepository> _mockSportRepository;
        private readonly Mock<ICourtPromotionRepository> _mockPromotionRepository;
        private readonly GetCourtsHandler _handler;

        public GetCourtsHandlerTests()
        {
            _mockCourtRepository = new Mock<ICourtRepository>();
            _mockSportRepository = new Mock<ISportRepository>();
            _mockPromotionRepository = new Mock<ICourtPromotionRepository>();
            _handler = new GetCourtsHandler(
                _mockCourtRepository.Object,
                _mockSportRepository.Object,
                _mockPromotionRepository.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_When_NoCourtExists()
        {
            // Arrange
            var paginationRequest = new PaginationRequest(0, 10);
            var query = new GetCourtsQuery(paginationRequest, null, null, null);

            // Setup method call to use GetAllCourtsAsync instead of GetPaginatedCourtsAsync
            _mockCourtRepository.Setup(r => r.GetAllCourtsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Court>());

            _mockSportRepository.Setup(r => r.GetSportsByIdsAsync(It.IsAny<List<SportId>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sport>());

            _mockPromotionRepository.Setup(r => r.GetPromotionsByCourtIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Courts.Data);
            _mockCourtRepository.Verify(r => r.GetAllCourtsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnCourtsList_When_CourtsExist()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var paginationRequest = new PaginationRequest(0, 10);
            var query = new GetCourtsQuery(paginationRequest, null, null, null);

            var court = Court.Create(
                CourtId.Of(courtId),
                new CourtName("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(sportId),
                TimeSpan.FromMinutes(100),
                "Main court", null,
                CourtType.Indoor,
                50, // MinDepositPercentage
                24  // CancellationWindowHours
            );

            // Properly create the Sport with the correct ID
            var sport = Sport.Create(
                SportId.Of(sportId),
                "Tennis",
                "Tennis sport",
                "icon"
            );

            _mockCourtRepository.Setup(r => r.GetAllCourtsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Court> { court });

            _mockSportRepository.Setup(r => r.GetSportsByIdsAsync(
                It.Is<List<SportId>>(ids => ids.Count > 0),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sport> { sport });

            _mockPromotionRepository.Setup(r => r.GetPromotionsByCourtIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Courts.Data);
            var courtDto = result.Courts.Data.First();
            Assert.Equal(courtId, courtDto.Id);
            Assert.Equal("Tennis Court 1", courtDto.CourtName);
            Assert.Equal("Main court", courtDto.Description);
            Assert.Equal(TimeSpan.FromMinutes(100), courtDto.SlotDuration);
            Assert.Equal(CourtType.Indoor, courtDto.CourtType);
            Assert.Equal(sportCenterId, courtDto.SportCenterId);
            Assert.Equal(sportId, courtDto.SportId);
            Assert.Equal("Tennis", courtDto.SportName);
            _mockCourtRepository.Verify(r => r.GetAllCourtsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
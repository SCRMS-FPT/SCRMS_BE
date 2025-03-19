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
        private readonly GetCourtsHandler _handler;

        public GetCourtsHandlerTests()
        {
            _mockCourtRepository = new Mock<ICourtRepository>();
            _mockSportRepository = new Mock<ISportRepository>();
            _handler = new GetCourtsHandler(_mockCourtRepository.Object, _mockSportRepository.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_When_NoCourtExists()
        {
            // Arrange
            var paginationRequest = new PaginationRequest(0, 10);
            var query = new GetCourtsQuery(paginationRequest, null, null, null);
            _mockCourtRepository.Setup(r => r.GetPaginatedCourtsAsync(0, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Court>());
            _mockSportRepository.Setup(r => r.GetSportsByIdsAsync(It.IsAny<List<SportId>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sport>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Courts.Data); // Fixed: Changed from PaginatedResult to Courts
            _mockCourtRepository.Verify(r => r.GetPaginatedCourtsAsync(0, 10, It.IsAny<CancellationToken>()), Times.Once);
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
                1,
                30
            );

            var sport = new Sport("Tennis", "Tennis sport", "icon");

            _mockCourtRepository.Setup(r => r.GetPaginatedCourtsAsync(0, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Court> { court });
            _mockSportRepository.Setup(r => r.GetSportsByIdsAsync(new List<SportId> { SportId.Of(sportId) }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Sport> { sport });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Courts.Data); // Fixed: Changed from PaginatedResult to Courts
            var courtDto = result.Courts.Data.ToList()[0];  // Fixed: Changed from PaginatedResult to Courts
            Assert.Equal(courtId, courtDto.Id);
            Assert.Equal("Tennis Court 1", courtDto.CourtName);
            Assert.Equal("Main court", courtDto.Description);
            Assert.Equal(TimeSpan.FromMinutes(100), courtDto.SlotDuration);
            Assert.Equal("Indoor", courtDto.CourtType.ToString());
            Assert.Equal(sportCenterId, courtDto.SportCenterId);
            Assert.Equal(sportId, courtDto.SportId);
            Assert.Equal("Tennis", courtDto.SportName);
            _mockCourtRepository.Verify(r => r.GetPaginatedCourtsAsync(0, 10, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
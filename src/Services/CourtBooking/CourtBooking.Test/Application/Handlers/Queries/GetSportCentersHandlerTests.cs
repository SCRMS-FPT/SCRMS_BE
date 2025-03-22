using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Moq;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetSportCentersHandlerTests
    {
        private readonly Mock<ISportCenterRepository> _mockSportCenterRepository;
        private readonly GetSportCentersHandler _handler;

        public GetSportCentersHandlerTests()
        {
            _mockSportCenterRepository = new Mock<ISportCenterRepository>();
            _handler = new GetSportCentersHandler(_mockSportCenterRepository.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnAllSportCenters_When_NoFiltersProvided()
        {
            // Arrange
            var paginationRequest = new PaginationRequest(0, 10);
            var query = new GetSportCentersQuery(paginationRequest);

            var sportCenterId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(ownerId),
                "Trung tâm Thể thao XYZ",
                "0987654321",
                new Location("123 Đường Thể thao", "Quận 1", "TP.HCM", "Việt Nam"),
                new GeoLocation(10.7756587, 106.7004238),
                new SportCenterImages("main-image.jpg", new List<string> { "image1.jpg", "image2.jpg" }),
                "Trung tâm thể thao hàng đầu"
            );

            var sportCenters = new List<SportCenter> { sportCenter };
            long totalCount = 1;

            _mockSportCenterRepository.Setup(r => r.GetPaginatedSportCentersAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenters);

            _mockSportCenterRepository.Setup(r => r.GetTotalSportCenterCountAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(totalCount);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SportCenters);
            Assert.Equal(0, result.SportCenters.PageIndex);
            Assert.Equal(10, result.SportCenters.PageSize);
            Assert.Equal(totalCount, result.SportCenters.Count);
            Assert.Single(result.SportCenters.Data);
            
            var sportCenterDto = result.SportCenters.Data.First();
            Assert.Equal(sportCenterId, sportCenterDto.Id);
            Assert.Equal(ownerId, sportCenterDto.OwnerId);
            Assert.Equal("Trung tâm Thể thao XYZ", sportCenterDto.Name);
            Assert.Equal("0987654321", sportCenterDto.PhoneNumber);
        }

        [Fact]
        public async Task Handle_Should_ReturnFilteredSportCenters_When_FiltersProvided()
        {
            // Arrange
            var paginationRequest = new PaginationRequest(0, 10);
            var city = "TP.HCM";
            var name = "XYZ";
            var query = new GetSportCentersQuery(paginationRequest, city, name);

            var sportCenterId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(ownerId),
                "Trung tâm Thể thao XYZ",
                "0987654321",
                new Location("123 Đường Thể thao", "Quận 1", "TP.HCM", "Việt Nam"),
                new GeoLocation(10.7756587, 106.7004238),
                new SportCenterImages("main-image.jpg", new List<string> { "image1.jpg", "image2.jpg" }),
                "Trung tâm thể thao hàng đầu"
            );

            var sportCenters = new List<SportCenter> { sportCenter };
            long totalCount = 1;

            _mockSportCenterRepository.Setup(r => r.GetFilteredPaginatedSportCentersAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenters);

            _mockSportCenterRepository.Setup(r => r.GetFilteredSportCenterCountAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(totalCount);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SportCenters);
            Assert.Equal(0, result.SportCenters.PageIndex);
            Assert.Equal(10, result.SportCenters.PageSize);
            Assert.Equal(totalCount, result.SportCenters.Count);
            Assert.Single(result.SportCenters.Data);
            
            var sportCenterDto = result.SportCenters.Data.First();
            Assert.Equal(sportCenterId, sportCenterDto.Id);
            Assert.Equal("Trung tâm Thể thao XYZ", sportCenterDto.Name);
            Assert.Equal("TP.HCM", sportCenterDto.City);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyResult_When_NoSportCentersFound()
        {
            // Arrange
            var paginationRequest = new PaginationRequest(0, 10);
            var query = new GetSportCentersQuery(paginationRequest);

            _mockSportCenterRepository.Setup(r => r.GetPaginatedSportCentersAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SportCenter>());

            _mockSportCenterRepository.Setup(r => r.GetTotalSportCenterCountAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0L);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SportCenters);
            Assert.Equal(0, result.SportCenters.PageIndex);
            Assert.Equal(10, result.SportCenters.PageSize);
            Assert.Equal(0, result.SportCenters.Count);
            Assert.Empty(result.SportCenters.Data);
        }
    }
}
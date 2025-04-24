using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenterById;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Moq;
using Xunit;
using BuildingBlocks.Exceptions;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetSportCenterByIdHandlerTests
    {
        private readonly Mock<ISportCenterRepository> _mockRepo;
        private readonly GetSportCenterByIdHandler _handler;

        public GetSportCenterByIdHandlerTests()
        {
            _mockRepo = new Mock<ISportCenterRepository>();
            _handler = new GetSportCenterByIdHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnSportCenterDetails_When_SportCenterExists()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddDays(-10);
            var lastModified = DateTime.UtcNow.AddDays(-5);

            // Tạo đối tượng Location, GeoLocation, SportCenterImages
            var location = Location.Of("123 Đường Thể thao", "TP.HCM", "Quận 1", "Việt Nam");
            var geoLocation = new GeoLocation(10.7756587, 106.7004238);
            var images = new SportCenterImages("main-image.jpg", new List<string> { "image1.jpg", "image2.jpg" });

            // Tạo SportCenter
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(ownerId),
                "Trung tâm Thể thao XYZ",
                "0987654321",
                location,
                geoLocation,
                images,
                "Trung tâm thể thao hàng đầu"
            );

            // Setup mock repository
            _mockRepo.Setup(r => r.GetSportCenterByIdAsync(It.Is<SportCenterId>(id => id.Value == sportCenterId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenter);

            var query = new GetSportCenterByIdQuery(sportCenterId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SportCenter);
            Assert.Equal(sportCenterId, result.SportCenter.Id);
            Assert.Equal(ownerId, result.SportCenter.OwnerId);
            Assert.Equal("Trung tâm Thể thao XYZ", result.SportCenter.Name);
            Assert.Equal("0987654321", result.SportCenter.PhoneNumber);
            Assert.Equal("123 Đường Thể thao", result.SportCenter.AddressLine);
            Assert.Equal("Quận 1", result.SportCenter.District);
            Assert.Equal("TP.HCM", result.SportCenter.City);
            Assert.Equal(10.7756587, result.SportCenter.Latitude);
            Assert.Equal(106.7004238, result.SportCenter.Longitude);
            Assert.Equal("main-image.jpg", result.SportCenter.Avatar);
            Assert.Equal(new List<string> { "image1.jpg", "image2.jpg" }, result.SportCenter.ImageUrls);
            Assert.Equal("Trung tâm thể thao hàng đầu", result.SportCenter.Description);
        }

        [Fact]
        public async Task Handle_Should_ThrowKeyNotFoundException_When_SportCenterNotFound()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            
            // Setup mock repository để trả về null
            _mockRepo.Setup(r => r.GetSportCenterByIdAsync(It.Is<SportCenterId>(id => id.Value == sportCenterId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SportCenter)null);

            var query = new GetSportCenterByIdQuery(sportCenterId);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }
    }
}
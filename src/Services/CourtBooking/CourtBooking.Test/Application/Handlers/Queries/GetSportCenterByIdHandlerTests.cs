using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.Data;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenterById;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CourtBooking.Application.Data.Repositories;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetSportCenterByIdHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<ISportCenterRepository> _mockRepo;
        private readonly GetSportCenterByIdHandler _handler;

        public GetSportCenterByIdHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
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
            var location = new Location("123 Đường Thể thao", "Quận 1", "TP.HCM", "Việt Nam");
            var geoLocation = new GeoLocation(10.7756587, 106.7004238);
            var images = new SportCenterImages("main-image.jpg", new List<string> { "image1.jpg", "image2.jpg" });

            // Sử dụng phương thức Create đúng của SportCenter
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

            // Thiết lập mock DbSet
            var mockDbSet = SetupMockDbSet(new List<SportCenter> { sportCenter });

            _mockContext.Setup(c => c.SportCenters).Returns(mockDbSet.Object);

            // Thiết lập mock FirstOrDefaultAsync
            mockDbSet
                .Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<SportCenter, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenter);

            var query = new GetSportCenterByIdQuery(sportCenterId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SportCenter);

            // Đảm bảo rằng các thuộc tính được kiểm tra phù hợp với SportCenterListDTO
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
            var mockDbSet = new Mock<DbSet<SportCenter>>();

            _mockContext.Setup(c => c.SportCenters).Returns(mockDbSet.Object);
            mockDbSet
                .Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<SportCenter, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SportCenter)null);

            var query = new GetSportCenterByIdQuery(sportCenterId);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }

        private Mock<DbSet<T>> SetupMockDbSet<T>(List<T> entities) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            var queryable = entities.AsQueryable();

            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return mockDbSet;
        }
    }
}
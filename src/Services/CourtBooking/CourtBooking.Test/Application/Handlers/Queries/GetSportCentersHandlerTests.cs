using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;
using CourtBooking.Application.Data;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Test.Common;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetSportCentersHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly GetSportCentersHandler _handler;

        public GetSportCentersHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new GetSportCentersHandler(_mockContext.Object);
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

            // Setup mock data
            var sportCenters = new List<SportCenter> { sportCenter };
            var mockDbSet = CreateMockDbSet(sportCenters);

            // Configure mocks
            _mockContext.Setup(c => c.SportCenters).Returns(mockDbSet.Object);

            // Correctly set up count method
            mockDbSet.Setup(m => m.LongCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenters.Count);

            // Setup async methods with TestAsyncEnumerable
            var courtsData = new List<Court>();
            var sportsData = new List<Sport>();
            var mockCourtsDbSet = CreateMockDbSet(courtsData);
            var mockSportsDbSet = CreateMockDbSet(sportsData);

            _mockContext.Setup(c => c.Courts).Returns(mockCourtsDbSet.Object);
            _mockContext.Setup(c => c.Sports).Returns(mockSportsDbSet.Object);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SportCenters);
            Assert.Equal(1, result.SportCenters.Count); // This should now pass
            Assert.Single(result.SportCenters.Data);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyResult_When_NoSportCentersFound()
        {
            // Arrange
            var paginationRequest = new PaginationRequest(0, 10);
            var query = new GetSportCentersQuery(paginationRequest);

            // Setup mock data with empty collections
            var emptySportCenters = new List<SportCenter>();
            var mockDbSet = CreateMockDbSet(emptySportCenters);

            // Configure mocks
            _mockContext.Setup(c => c.SportCenters).Returns(mockDbSet.Object);

            // Setup empty courts and sports
            var emptyCourts = new List<Court>();
            var emptySports = new List<Sport>();
            var mockCourtsDbSet = CreateMockDbSet(emptyCourts);
            var mockSportsDbSet = CreateMockDbSet(emptySports);

            _mockContext.Setup(c => c.Courts).Returns(mockCourtsDbSet.Object);
            _mockContext.Setup(c => c.Sports).Returns(mockSportsDbSet.Object);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SportCenters);
            Assert.Empty(result.SportCenters.Data);
            Assert.Equal(0, result.SportCenters.Count);
        }

        [Fact]
        public async Task Handle_Should_FilterByCityAndName_WhenFiltersProvided()
        {
            // Arrange
            var paginationRequest = new PaginationRequest(0, 10);
            var city = "HCMC";
            var name = "XYZ";
            var query = new GetSportCentersQuery(paginationRequest, city, name);

            // Setup data that matches the filter
            var sportCenterId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(ownerId),
                "Trung tâm Thể thao XYZ",
                "0987654321",
                new Location("123 Đường Thể thao", "HCMC", "Quận 1", "Phường 1"),
                new GeoLocation(10.7756587, 106.7004238),
                new SportCenterImages("main-image.jpg", new List<string> { "image1.jpg", "image2.jpg" }),
                "Trung tâm thể thao hàng đầu"
            );

            var sportCenters = new List<SportCenter> { sportCenter };
            var mockDbSet = CreateMockDbSet(sportCenters);

            // Configure mocks
            _mockContext.Setup(c => c.SportCenters).Returns(mockDbSet.Object);

            // Setup empty courts and sports
            var emptyCourts = new List<Court>();
            var emptySports = new List<Sport>();
            var mockCourtsDbSet = CreateMockDbSet(emptyCourts);
            var mockSportsDbSet = CreateMockDbSet(emptySports);

            _mockContext.Setup(c => c.Courts).Returns(mockCourtsDbSet.Object);
            _mockContext.Setup(c => c.Sports).Returns(mockSportsDbSet.Object);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SportCenters);
            Assert.NotEmpty(result.SportCenters.Data);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            var queryable = data.AsQueryable();
            var asyncQueryable = data.AsQueryable().AsAsyncQueryable();

            // Basic IQueryable setup
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            // Setup Count method
            mockDbSet.Setup(m => m.LongCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(data.Count);

            // Thiết lập IAsyncEnumerable
            mockDbSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            // Thiết lập IQueryable<T> cho phương thức LongCountAsync
            mockDbSet.Setup(m => m.AsQueryable()).Returns(asyncQueryable);

            // Thiết lập phương thức FindAsync
            mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .Returns<object[]>(ids =>
                {
                    var idValue = ids[0];
                    var item = data.FirstOrDefault();
                    return ValueTask.FromResult(item);
                });

            return mockDbSet;
        }
    }
}
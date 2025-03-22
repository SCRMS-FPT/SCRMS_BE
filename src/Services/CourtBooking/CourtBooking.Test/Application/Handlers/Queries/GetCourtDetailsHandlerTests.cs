using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CourtBooking.Application.CourtManagement.Queries.GetCourtDetails;
using CourtBooking.Application.Data;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetCourtDetailsHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly GetCourtDetailsHandler _handler;

        public GetCourtDetailsHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new GetCourtDetailsHandler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnCourtDetails_When_CourtExists()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddDays(-10);
            var lastModified = DateTime.UtcNow.AddDays(-5);

            // Mock facilities data
            var facilitiesData = new List<FacilityDTO>
            {
                new FacilityDTO { Name = "Wifi", Description = "true" },
                new FacilityDTO { Name = "Phòng thay đồ",Description = "true" }
            };
            var facilitiesJson = JsonSerializer.Serialize(facilitiesData);

            // Create court entity
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Sân tennis 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(sportId),
                TimeSpan.FromMinutes(60),
                "Sân tennis chính",
                facilitiesJson,
                CourtType.Indoor,
                50m
            );

            // Create related sport entity
            // Replace the problematic line with:
            var sport = new Sport("Tennis", "Tennis description", "tennis.jpg");
            // Or if there's a factory method:
            // var sport = Sport.Create(SportId.Of(sportId), "Tennis", "Tennis description", "tennis.jpg");

            // Create related sport center entity
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(Guid.NewGuid()),
                "Trung tâm Thể thao ABC",
                "0987654321",
                new Location("123 Đường Thể thao", "Quận 1", "TP.HCM", "Việt Nam"),
                new GeoLocation(10.7756587, 106.7004238),
                new SportCenterImages("main-image.jpg", new List<string> { "image1.jpg", "image2.jpg" }),
                "Trung tâm thể thao hàng đầu"
            );

            // Setup DbSets
            var mockCourtDbSet = SetupMockDbSet(new List<Court> { court });
            var mockSportDbSet = SetupMockDbSet(new List<Sport> { sport });
            var mockSportCenterDbSet = SetupMockDbSet(new List<SportCenter> { sportCenter });

            _mockContext.Setup(c => c.Courts).Returns(mockCourtDbSet.Object);
            _mockContext.Setup(c => c.Sports).Returns(mockSportDbSet.Object);
            _mockContext.Setup(c => c.SportCenters).Returns(mockSportCenterDbSet.Object);

            // Setup Include for CourtSchedules
            mockCourtDbSet.Setup(d => d.Include(It.IsAny<string>())).Returns(mockCourtDbSet.Object);

            // Setup FirstOrDefaultAsync for each DbSet
            mockCourtDbSet
                .Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<Court, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            mockSportDbSet
                .Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<Sport, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sport);

            mockSportCenterDbSet
                .Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<SportCenter, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportCenter);

            var query = new GetCourtDetailsQuery(courtId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Court);
            Assert.Equal(courtId, result.Court.Id);
            Assert.Equal("Sân tennis 1", result.Court.CourtName);
            Assert.Equal(sportId, result.Court.SportId);
            Assert.Equal(sportCenterId, result.Court.SportCenterId);
            Assert.Equal("Sân tennis chính", result.Court.Description);
            Assert.Equal(TimeSpan.FromMinutes(60), result.Court.SlotDuration);
            Assert.Equal(CourtStatus.Open, result.Court.Status);
            Assert.Equal(CourtType.Indoor, result.Court.CourtType);
            Assert.Equal("Tennis", result.Court.SportName);
            Assert.Equal("Trung tâm Thể thao ABC", result.Court.SportCenterName);
            Assert.Equal(50m, result.Court.MinDepositPercentage);
            Assert.NotNull(result.Court.Facilities);
            Assert.Equal(2, result.Court.Facilities.Count);
            Assert.Contains(result.Court.Facilities, f => f.Name == "Wifi");
            Assert.Contains(result.Court.Facilities, f => f.Name == "Phòng thay đồ");
        }

        [Fact]
        public async Task Handle_Should_ThrowKeyNotFoundException_When_CourtNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var mockCourtDbSet = new Mock<DbSet<Court>>();

            _mockContext.Setup(c => c.Courts).Returns(mockCourtDbSet.Object);
            mockCourtDbSet.Setup(d => d.Include(It.IsAny<string>())).Returns(mockCourtDbSet.Object);
            mockCourtDbSet
                .Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<Court, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Court)null);

            var query = new GetCourtDetailsQuery(courtId);

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
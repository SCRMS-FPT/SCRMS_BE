using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.CourtManagement.Queries.GetCourtDetails;
using CourtBooking.Application.Data;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using MockQueryable.Moq;

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
            var courtId = CourtId.Of(Guid.NewGuid());
            var sportCenterId = SportCenterId.Of(Guid.NewGuid());
            var sportId = SportId.Of(Guid.NewGuid());
            
            // Tạo đối tượng court
            var court = Court.Create(
                courtId,
                CourtName.Of("Test Court"),
                sportCenterId,
                sportId,
                TimeSpan.FromHours(1),
                "Test Description",
                "[]", // JSON array rỗng cho facilities
                CourtType.Indoor,
                50
            );

            // Tạo đối tượng sport
            var sport = Sport.Create(
                sportId,
                "Test Sport",
                "Sport Description",
                "sport-icon.png"
            );

            // Tạo đối tượng sportCenter
            var sportCenter = SportCenter.Create(
                sportCenterId,
                OwnerId.Of(Guid.NewGuid()),
                "Test Sport Center",
                "0123456789",
                Location.Of("123 Main St", "City", "District", "Commune"),
                new GeoLocation(10.0, 20.0),
                new SportCenterImages("avatar.jpg", new List<string>()),
                "Sport Center Description"
            );

            // Mocking với MockQueryable
            var courts = new List<Court> { court }.AsQueryable();
            var mockCourtsDbSet = courts.BuildMockDbSet();
            _mockContext.Setup(c => c.Courts).Returns(mockCourtsDbSet.Object);

            var sports = new List<Sport> { sport }.AsQueryable();
            var mockSportsDbSet = sports.BuildMockDbSet();
            _mockContext.Setup(c => c.Sports).Returns(mockSportsDbSet.Object);

            var sportCenters = new List<SportCenter> { sportCenter }.AsQueryable();
            var mockSportCentersDbSet = sportCenters.BuildMockDbSet();
            _mockContext.Setup(c => c.SportCenters).Returns(mockSportCentersDbSet.Object);

            var promotions = new List<CourtPromotion>().AsQueryable();
            var mockPromotionsDbSet = promotions.BuildMockDbSet();
            _mockContext.Setup(c => c.CourtPromotions).Returns(mockPromotionsDbSet.Object);

            // Act
            var result = await _handler.Handle(new GetCourtDetailsQuery(courtId.Value), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(courtId.Value, result.Court.Id);
            Assert.Equal(court.CourtName.Value, result.Court.CourtName);
            Assert.Equal(sport.Name, result.Court.SportName);
            Assert.Equal(sportCenter.Name, result.Court.SportCenterName);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_When_CourtNotFound()
        {
            // Arrange
            var courtId = CourtId.Of(Guid.NewGuid());
            
            // Mock an empty court collection
            var emptyCourts = new List<Court>().AsQueryable();
            var mockCourtsDbSet = emptyCourts.BuildMockDbSet();
            _mockContext.Setup(c => c.Courts).Returns(mockCourtsDbSet.Object);

            // Set up required DbSets for complete context
            var emptySports = new List<Sport>().AsQueryable();
            var mockSportsDbSet = emptySports.BuildMockDbSet();
            _mockContext.Setup(c => c.Sports).Returns(mockSportsDbSet.Object);

            var emptySportCenters = new List<SportCenter>().AsQueryable();
            var mockSportCentersDbSet = emptySportCenters.BuildMockDbSet();
            _mockContext.Setup(c => c.SportCenters).Returns(mockSportCentersDbSet.Object);

            var emptyPromotions = new List<CourtPromotion>().AsQueryable();
            var mockPromotionsDbSet = emptyPromotions.BuildMockDbSet();
            _mockContext.Setup(c => c.CourtPromotions).Returns(mockPromotionsDbSet.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(new GetCourtDetailsQuery(courtId.Value), CancellationToken.None));
        }
    }
}
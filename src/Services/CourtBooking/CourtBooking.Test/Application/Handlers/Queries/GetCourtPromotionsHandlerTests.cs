using CourtBooking.Application.CourtManagement.Queries.GetCourtPromotions;
using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using CourtBooking.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BuildingBlocks.Exceptions;

namespace CourtBooking.Test.Application.Handlers.Queries
{
    public class GetCourtPromotionsHandlerTests
    {
        private readonly Mock<ICourtPromotionRepository> _mockPromotionRepository;
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly Mock<IApplicationDbContext> _mockDbContext;
        private readonly Mock<DbSet<SportCenter>> _mockSportCentersDbSet;
        private readonly GetCourtPromotionsHandler _handler;

        public GetCourtPromotionsHandlerTests()
        {
            _mockPromotionRepository = new Mock<ICourtPromotionRepository>();
            _mockCourtRepository = new Mock<ICourtRepository>();
            _mockDbContext = new Mock<IApplicationDbContext>();
            _mockSportCentersDbSet = new Mock<DbSet<SportCenter>>();

            _mockDbContext.Setup(c => c.SportCenters).Returns(_mockSportCentersDbSet.Object);

            _handler = new GetCourtPromotionsHandler(
                _mockPromotionRepository.Object,
                _mockCourtRepository.Object,
                _mockDbContext.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_When_NoPromotionsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var query = new GetCourtPromotionsQuery(courtId, userId, "User");

            // Setup mock court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Test Court"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test description",
                null,
                CourtType.Indoor,
                50
            );
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            _mockPromotionRepository.Setup(r => r.GetPromotionsByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result);

            _mockPromotionRepository.Verify(r => r.GetPromotionsByCourtIdAsync(
                CourtId.Of(courtId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnPromotionsList_When_PromotionsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            var query = new GetCourtPromotionsQuery(courtId, userId, "User");

            // Setup mock court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Test Court"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test description",
                null,
                CourtType.Indoor,
                50
            );
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            var promotion = CourtPromotion.Create(
                CourtId.Of(courtId),
                "Discount for summer season",
                "Percentage",
                20.0m,
                DateTime.Today,
                DateTime.Today.AddMonths(3)
            );

            // Sử dụng reflection để thiết lập ID vì ID được gán trong hàm Create
            typeof(CourtPromotion).GetProperty("Id").SetValue(promotion, CourtPromotionId.Of(promotionId));

            _mockPromotionRepository.Setup(r => r.GetPromotionsByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion> { promotion });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result);
            var promotionDto = result[0];
            Assert.Equal(promotionId, promotionDto.Id);
            Assert.Equal(courtId, promotionDto.CourtId);
            Assert.Equal("Discount for summer season", promotionDto.Description);
            Assert.Equal(20.0m, promotionDto.DiscountValue);
            Assert.Equal("Percentage", promotionDto.DiscountType);
            Assert.Equal(DateTime.Today, promotionDto.ValidFrom);
            Assert.Equal(DateTime.Today.AddMonths(3), promotionDto.ValidTo);

            _mockPromotionRepository.Verify(r => r.GetPromotionsByCourtIdAsync(
                CourtId.Of(courtId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_MapDiscountType_When_PromotionsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var query = new GetCourtPromotionsQuery(courtId, userId, "User");

            // Setup mock court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Test Court"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test description",
                null,
                CourtType.Indoor,
                50
            );
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            var percentagePromotion = CourtPromotion.Create(
                CourtId.Of(courtId),
                "Discount as percentage",
                "Percentage",
                20.0m,
                DateTime.Today,
                DateTime.Today.AddMonths(3)
            );

            var fixedPromotion = CourtPromotion.Create(
                CourtId.Of(courtId),
                "Fixed amount discount",
                "FixedAmount",
                50000.0m,
                DateTime.Today,
                DateTime.Today.AddMonths(3)
            );

            _mockPromotionRepository.Setup(r => r.GetPromotionsByCourtIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourtPromotion> { percentagePromotion, fixedPromotion });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Percentage", result[0].DiscountType);
            Assert.Equal("FixedAmount", result[1].DiscountType);
        }

        [Fact]
        public async Task Handle_Should_ThrowNotFound_When_CourtDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var query = new GetCourtPromotionsQuery(courtId, userId, "User");

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Court)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Should_ThrowUnauthorized_When_CourtOwnerDoesNotOwnCourt()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var query = new GetCourtPromotionsQuery(courtId, userId, "CourtOwner");

            // Setup mock court
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Test Court"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test description",
                null,
                CourtType.Indoor,
                50
            );
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(
                    It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Setup mock sport center with different owner
            var anotherOwnerId = Guid.NewGuid();
            var sportCenters = new List<SportCenter>
            {
                SportCenter.Create(
                    SportCenterId.Of(sportCenterId),
                    OwnerId.Of(anotherOwnerId), // Not the requesting user
                    "Test Center",
                    "1234567890",
                    new Location("Address", "City", "Country", "PostalCode"),
                    new GeoLocation(0.0, 0.0),
                    new SportCenterImages("avatar.jpg", new List<string>()),
                    "Description"
                )
            }.AsQueryable();

            SetupMockDbSet(_mockSportCentersDbSet, sportCenters);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }

        private void SetupMockDbSet<T>(Mock<DbSet<T>> mockDbSet, IQueryable<T> data) where T : class
        {
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            // Instead of trying to mock FirstOrDefaultAsync directly, we'll mock the necessary methods
            // to allow LINQ queries to work properly on our mock
            mockDbSet.Setup(x => x.Find(It.IsAny<object[]>()))
                .Returns<object[]>(ids => data.FirstOrDefault(d =>
                    typeof(T).GetProperty("Id").GetValue(d).Equals(ids[0])));
        }
    }
}
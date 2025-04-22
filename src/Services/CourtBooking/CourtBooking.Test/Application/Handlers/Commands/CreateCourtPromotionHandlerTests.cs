using BuildingBlocks.Exceptions;
using CourtBooking.Application.CourtManagement.Command.CreateCourtPromotion;
using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MockQueryable.Moq;

namespace CourtBooking.Test.Application.Handlers.Commands
{
    public class CreateCourtPromotionHandlerTests
    {
        private readonly Mock<ICourtPromotionRepository> _mockPromotionRepository;
        private readonly Mock<ICourtRepository> _mockCourtRepository;
        private readonly Mock<IApplicationDbContext> _mockDbContext;
        private readonly CreateCourtPromotionHandler _handler;

        public CreateCourtPromotionHandlerTests()
        {
            _mockPromotionRepository = new Mock<ICourtPromotionRepository>();
            _mockCourtRepository = new Mock<ICourtRepository>();
            _mockDbContext = new Mock<IApplicationDbContext>();

            // Tạo mock cho DbSet SportCenter
            var mockSportCentersDbSet = new Mock<DbSet<SportCenter>>();
            _mockDbContext.Setup(c => c.SportCenters).Returns(mockSportCentersDbSet.Object);

            _handler = new CreateCourtPromotionHandler(
                _mockPromotionRepository.Object,
                _mockCourtRepository.Object,
                _mockDbContext.Object
            );
        }

        [Fact]
        public async Task Handle_Should_CreatePromotion_When_Valid()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            var now = DateTime.Now;

            var command = new CreateCourtPromotionCommand(
                courtId,
                "Discount for summer season",
                "Percentage",
                20.0m,
                DateTime.Today,
                DateTime.Today.AddMonths(3),
                userId
            );

            // Sử dụng phương thức Create của Court thay vì khởi tạo trực tiếp
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test description",
                "Test facilities",
                CourtType.Indoor,
                100
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Sử dụng phương thức Create của SportCenter thay vì khởi tạo trực tiếp
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(userId),
                "Test Sport Center",
                "0987654321",
                new Location("Test Street", "Test District", "Test City", "Test Country"),
                new GeoLocation(10.0, 10.0),
                new SportCenterImages("main-image.jpg", new List<string>()),
                "Test Description"
            );

            // Create mock DbSet with proper setup for async operations
            var sportCenters = new List<SportCenter> { sportCenter }.AsQueryable();
            var mockDbSet = sportCenters.BuildMockDbSet();
            _mockDbContext.Setup(c => c.SportCenters).Returns(mockDbSet.Object);

            // Setup repository to capture the created promotion
            CourtPromotion addedPromotion = null;
            _mockPromotionRepository.Setup(r => r.AddAsync(It.IsAny<CourtPromotion>(), It.IsAny<CancellationToken>()))
                .Callback<CourtPromotion, CancellationToken>((p, _) =>
                {
                    addedPromotion = p;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(courtId, result.CourtId);
            Assert.Equal("Discount for summer season", result.Description);
            Assert.Equal("Percentage", result.DiscountType);
            Assert.Equal(20.0m, result.DiscountValue);
            Assert.Equal(DateTime.Today, result.ValidFrom);
            Assert.Equal(DateTime.Today.AddMonths(3), result.ValidTo);

            // Verify repository calls
            _mockPromotionRepository.Verify(r => r.AddAsync(It.IsAny<CourtPromotion>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ThrowUnauthorizedAccessException_When_UserNotOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();

            var command = new CreateCourtPromotionCommand(
                courtId,
                "Discount for summer season",
                "Percentage",
                20.0m,
                DateTime.Today,
                DateTime.Today.AddMonths(3),
                userId
            );

            // Sử dụng phương thức Create của Court thay vì khởi tạo trực tiếp
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test description",
                "Test facilities",
                CourtType.Indoor,
                100
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Sử dụng phương thức Create của SportCenter thay vì khởi tạo trực tiếp - với id khác
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(anotherUserId), // Khác với userId trong command
                "Test Sport Center",
                "0987654321",
                new Location("Test Street", "Test District", "Test City", "Test Country"),
                new GeoLocation(10.0, 10.0),
                new SportCenterImages("main-image.jpg", new List<string>()),
                "Test Description"
            );

            // Create mock DbSet with proper setup for async operations
            var sportCenters = new List<SportCenter> { sportCenter }.AsQueryable();
            var mockDbSet = sportCenters.BuildMockDbSet();
            _mockDbContext.Setup(c => c.SportCenters).Returns(mockDbSet.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            // Verify repository calls
            _mockPromotionRepository.Verify(r => r.AddAsync(It.IsAny<CourtPromotion>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ThrowNotFoundException_When_CourtNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var command = new CreateCourtPromotionCommand(
                courtId,
                "Discount for summer season",
                "Percentage",
                20.0m,
                DateTime.Today,
                DateTime.Today.AddMonths(3),
                userId
            );

            // Setup court not found
            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Court)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            // Verify repository calls
            _mockPromotionRepository.Verify(r => r.AddAsync(It.IsAny<CourtPromotion>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ThrowArgumentException_When_InvalidPercentage()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var command = new CreateCourtPromotionCommand(
                courtId,
                "Discount for summer season",
                "Percentage",
                120.0m, // Giá trị phần trăm không hợp lệ (trên 100%)
                DateTime.Today,
                DateTime.Today.AddMonths(3),
                userId
            );

            // Sử dụng phương thức Create của Court thay vì khởi tạo trực tiếp
            var court = Court.Create(
                CourtId.Of(courtId),
                CourtName.Of("Tennis Court 1"),
                SportCenterId.Of(sportCenterId),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Test description",
                "Test facilities",
                CourtType.Indoor,
                100
            );

            _mockCourtRepository.Setup(r => r.GetCourtByIdAsync(It.IsAny<CourtId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(court);

            // Sử dụng phương thức Create của SportCenter thay vì khởi tạo trực tiếp
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(sportCenterId),
                OwnerId.Of(userId),
                "Test Sport Center",
                "0987654321",
                new Location("Test Street", "Test District", "Test City", "Test Country"),
                new GeoLocation(10.0, 10.0),
                new SportCenterImages("main-image.jpg", new List<string>()),
                "Test Description"
            );

            // Create mock DbSet with proper setup for async operations
            var sportCenters = new List<SportCenter> { sportCenter }.AsQueryable();
            var mockDbSet = sportCenters.BuildMockDbSet();
            _mockDbContext.Setup(c => c.SportCenters).Returns(mockDbSet.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }
    }
}
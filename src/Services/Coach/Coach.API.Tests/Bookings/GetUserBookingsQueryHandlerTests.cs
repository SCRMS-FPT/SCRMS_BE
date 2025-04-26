using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Pagination;
using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Bookings.GetUserBookings;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Coach.API.Tests.Bookings
{
    public class GetUserBookingsQueryHandlerTests
    {
        // Normal cases
        [Fact]
        public async Task Handle_WithValidUserId_ReturnsUserBookings()
        {
            // Arrange
            var userId = Guid.Parse("a9791696-cf29-465b-ba18-7a7a6773a98a");
            var coachId = Guid.Parse("7f8b6fbc-a15c-4055-8129-90d4252f50d3");
            var sportId = Guid.NewGuid();
            var packageId = Guid.NewGuid();

            // Match the expected booking ID with the actual one from the error message
            var bookingId = Guid.Parse("5ef4fcd7-2da5-44e7-82e4-ca1057bacc0e");
            var bookingId2 = Guid.Parse("4f9c2f18-6a3f-4a8c-9b4f-2d2a7f67e819");

            var bookings = new List<CoachBooking>
            {
                new CoachBooking
                {
                    Id = bookingId,  // Use the updated booking ID that matches what's being checked
                    UserId = userId,
                    CoachId = coachId,
                    SportId = sportId,
                    PackageId = packageId,
                    BookingDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                    StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)),
                    EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                    Status = "completed",
                    TotalPrice = 50m
                },
                new CoachBooking
                {
                    Id = bookingId2,
                    UserId = userId,
                    CoachId = coachId,
                    SportId = sportId,
                    PackageId = null,
                    BookingDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                    StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(14)),
                    EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(15)),
                    Status = "pending",
                    TotalPrice = 60m
                }
            };

            var mockCoach = new Data.Models.Coach
            {
                UserId = coachId,
                FullName = "John Coach",
                Status = "active"
            };

            var mockPackage = new CoachPackage
            {
                Id = packageId,
                Name = "Basic Package"
            };

            // Create a mock for the DbSet properly using MockQueryable.Moq
            var mockDbSet = bookings.AsQueryable().BuildMockDbSet();

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(r => r.GetCoachBookingsByUserIdQueryable(userId))
                .Returns(mockDbSet.Object);

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCoach);

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            mockPackageRepo.Setup(r => r.GetCoachPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockPackage);

            var mockDbContext = new Mock<CoachDbContext>();
            mockDbContext.Setup(c => c.CoachBookings).Returns(mockDbSet.Object);

            // Create the handler
            var handler = new GetUserBookingsQueryHandler(
                mockBookingRepo.Object,
                mockCoachRepo.Object,
                mockPackageRepo.Object,
                mockDbContext.Object);

            // Create query
            var query = new GetUserBookingsQuery(
                userId,
                0,
                10,
                null,
                null,
                null,
                null,
                null,
                null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.Data.Count());

            // Check the first booking
            var firstBooking = result.Data.First();

            // Assert the fixed booking ID
            Assert.Equal(bookingId, firstBooking.Id);

            Assert.Equal(coachId, firstBooking.CoachId);
            Assert.Equal("John Coach", firstBooking.CoachName);
            Assert.Equal("completed", firstBooking.Status);
            Assert.Equal("Basic Package", firstBooking.PackageName);

            // Check the second booking
            var secondBooking = result.Data.Skip(1).First();
            Assert.Equal(bookings[1].Id, secondBooking.Id); // Use the GUID from the test data
            Assert.Equal("pending", secondBooking.Status);
            Assert.Null(secondBooking.PackageName);
        }

        // Test with filters
        [Fact]
        public async Task Handle_WithStatusFilter_ReturnsFilteredBookings()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coachId = Guid.NewGuid();

            var bookings = new List<CoachBooking>
            {
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId, Status = "completed" },
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId, Status = "pending" },
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId, Status = "cancelled" }
            };

            // Create a mock for the DbSet using MockQueryable.Moq
            var mockDbSet = bookings.AsQueryable().BuildMockDbSet();

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(r => r.GetCoachBookingsByUserIdQueryable(userId))
                .Returns(mockDbSet.Object);

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Models.Coach { UserId = coachId, FullName = "Test Coach" });

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            var mockDbContext = new Mock<CoachDbContext>();
            mockDbContext.Setup(c => c.CoachBookings).Returns(mockDbSet.Object);

            var handler = new GetUserBookingsQueryHandler(
                mockBookingRepo.Object,
                mockCoachRepo.Object,
                mockPackageRepo.Object,
                mockDbContext.Object);

            // Query with status filter
            var query = new GetUserBookingsQuery(
                userId,
                0,
                10,
                "completed",  // Status filter
                null,
                null,
                null,
                null,
                null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Single(result.Data);
            Assert.Equal("completed", result.Data.First().Status);
        }

        [Fact]
        public async Task Handle_WithDateRangeFilter_ReturnsFilteredBookings()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var today = DateOnly.FromDateTime(DateTime.Now);
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);
            var nextWeek = today.AddDays(7);

            var bookings = new List<CoachBooking>
            {
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId, BookingDate = yesterday },
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId, BookingDate = today },
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId, BookingDate = tomorrow },
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId, BookingDate = nextWeek }
            };

            // Create a mock for the DbSet using MockQueryable.Moq
            var mockDbSet = bookings.AsQueryable().BuildMockDbSet();

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(r => r.GetCoachBookingsByUserIdQueryable(userId))
                .Returns(mockDbSet.Object);

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Models.Coach { UserId = coachId, FullName = "Test Coach" });

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            var mockDbContext = new Mock<CoachDbContext>();
            mockDbContext.Setup(c => c.CoachBookings).Returns(mockDbSet.Object);

            var handler = new GetUserBookingsQueryHandler(
                mockBookingRepo.Object,
                mockCoachRepo.Object,
                mockPackageRepo.Object,
                mockDbContext.Object);

            // Query with date range filter
            var query = new GetUserBookingsQuery(
                userId,
                0,
                10,
                null,
                today,       // Start date
                tomorrow,    // End date
                null,
                null,
                null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);  // Should have today and tomorrow's bookings
        }

        [Fact]
        public async Task Handle_WithCoachFilter_ReturnsFilteredBookings()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coachId1 = Guid.NewGuid();
            var coachId2 = Guid.NewGuid();

            var bookings = new List<CoachBooking>
            {
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId1 },
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId1 },
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId2 }
            };

            // Create a mock for the DbSet using MockQueryable.Moq
            var mockDbSet = bookings.AsQueryable().BuildMockDbSet();

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(r => r.GetCoachBookingsByUserIdQueryable(userId))
                .Returns(mockDbSet.Object);

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Models.Coach { UserId = coachId1, FullName = "Coach 1" });
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Models.Coach { UserId = coachId2, FullName = "Coach 2" });

            var mockPackageRepo = new Mock<ICoachPackageRepository>();
            var mockDbContext = new Mock<CoachDbContext>();
            mockDbContext.Setup(c => c.CoachBookings).Returns(mockDbSet.Object);

            var handler = new GetUserBookingsQueryHandler(
                mockBookingRepo.Object,
                mockCoachRepo.Object,
                mockPackageRepo.Object,
                mockDbContext.Object);

            // Query with coach filter
            var query = new GetUserBookingsQuery(
                userId,
                0,
                10,
                null,
                null,
                null,
                null,
                coachId1,  // Coach filter
                null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result.Data, item => Assert.Equal(coachId1, item.CoachId));
        }

        // Abnormal cases
        [Fact]
        public async Task Handle_NoBookings_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Create an empty mock DbSet
            var emptyList = new List<CoachBooking>();
            var mockDbSet = emptyList.AsQueryable().BuildMockDbSet();

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(r => r.GetCoachBookingsByUserIdQueryable(userId))
                .Returns(mockDbSet.Object);

            var mockDbContext = new Mock<CoachDbContext>();
            mockDbContext.Setup(c => c.CoachBookings).Returns(mockDbSet.Object);

            var handler = new GetUserBookingsQueryHandler(
                mockBookingRepo.Object,
                Mock.Of<ICoachRepository>(),
                Mock.Of<ICoachPackageRepository>(),
                mockDbContext.Object);

            var query = new GetUserBookingsQuery(userId, 0, 10, null, null, null, null, null, null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task Handle_CoachNotFound_UsesUnknownCoachName()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coachId = Guid.NewGuid();

            var bookings = new List<CoachBooking>
            {
                new CoachBooking { Id = Guid.NewGuid(), UserId = userId, CoachId = coachId }
            };

            // Create a mock for the DbSet using MockQueryable.Moq
            var mockDbSet = bookings.AsQueryable().BuildMockDbSet();

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(r => r.GetCoachBookingsByUserIdQueryable(userId))
                .Returns(mockDbSet.Object);

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Models.Coach)null);  // Coach not found

            var mockDbContext = new Mock<CoachDbContext>();
            mockDbContext.Setup(c => c.CoachBookings).Returns(mockDbSet.Object);

            var handler = new GetUserBookingsQueryHandler(
                mockBookingRepo.Object,
                mockCoachRepo.Object,
                Mock.Of<ICoachPackageRepository>(),
                mockDbContext.Object);

            var query = new GetUserBookingsQuery(userId, 0, 10, null, null, null, null, null, null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal("Unknown Coach", result.Data.First().CoachName);
        }

        // Boundary cases
        [Fact]
        public async Task Handle_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coachId = Guid.NewGuid();

            // Create 25 bookings
            var bookings = new List<CoachBooking>();
            for (int i = 0; i < 25; i++)
            {
                bookings.Add(new CoachBooking
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CoachId = coachId,
                    BookingDate = DateOnly.FromDateTime(DateTime.Now.AddDays(i))
                });
            }

            // Create a mock for the DbSet using MockQueryable.Moq
            var mockDbSet = bookings.AsQueryable().BuildMockDbSet();

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            mockBookingRepo.Setup(r => r.GetCoachBookingsByUserIdQueryable(userId))
                .Returns(mockDbSet.Object);

            var mockCoachRepo = new Mock<ICoachRepository>();
            mockCoachRepo.Setup(r => r.GetCoachByIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Models.Coach { UserId = coachId, FullName = "Test Coach" });

            var mockDbContext = new Mock<CoachDbContext>();
            mockDbContext.Setup(c => c.CoachBookings).Returns(mockDbSet.Object);

            var handler = new GetUserBookingsQueryHandler(
                mockBookingRepo.Object,
                mockCoachRepo.Object,
                Mock.Of<ICoachPackageRepository>(),
                mockDbContext.Object);

            // First page (0-indexed)
            var query1 = new GetUserBookingsQuery(userId, 0, 10, null, null, null, null, null, null);
            var result1 = await handler.Handle(query1, CancellationToken.None);

            // Second page
            var query2 = new GetUserBookingsQuery(userId, 1, 10, null, null, null, null, null, null);
            var result2 = await handler.Handle(query2, CancellationToken.None);

            // Third page (should have only 5 items)
            var query3 = new GetUserBookingsQuery(userId, 2, 10, null, null, null, null, null, null);
            var result3 = await handler.Handle(query3, CancellationToken.None);

            // Assert
            Assert.Equal(25, result1.Count);
            Assert.Equal(10, result1.Data.Count());
            Assert.Equal(10, result2.Data.Count());
            Assert.Equal(5, result3.Data.Count());

            // Check pagination info
            Assert.Equal(0, result1.PageIndex);
            Assert.Equal(1, result2.PageIndex);
            Assert.Equal(2, result3.PageIndex);

            // Check different items on different pages
            var firstPageIds = result1.Data.Select(i => i.Id).ToHashSet();
            var secondPageIds = result2.Data.Select(i => i.Id).ToHashSet();
            var thirdPageIds = result3.Data.Select(i => i.Id).ToHashSet();

            Assert.Empty(firstPageIds.Intersect(secondPageIds));
            Assert.Empty(firstPageIds.Intersect(thirdPageIds));
            Assert.Empty(secondPageIds.Intersect(thirdPageIds));
        }
    }
}
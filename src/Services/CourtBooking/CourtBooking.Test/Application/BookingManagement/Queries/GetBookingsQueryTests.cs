using CourtBooking.Application.BookingManagement.Queries.GetBookings;
using CourtBooking.Domain.Enums;
using System;
using Xunit;

namespace CourtBooking.Test.Application.BookingManagement.Queries
{
    public class GetBookingsQueryTests
    {
        [Fact]
        public void Constructor_Should_CreateInstance_When_CalledWithValidParameters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = "User";
            Guid? filterUserId = Guid.NewGuid();
            Guid? courtId = Guid.NewGuid();
            Guid? sportsCenterId = Guid.NewGuid();
            BookingStatus? status = BookingStatus.Deposited; // Changed from Confirmed to Deposited
            DateTime? startDate = DateTime.Today;
            DateTime? endDate = DateTime.Today.AddDays(7);
            int page = 0;
            int limit = 10;

            // Act
            var query = new GetBookingsQuery(
                userId,
                role,
                "User",
                filterUserId,
                courtId,
                sportsCenterId,
                status,
                startDate,
                endDate,
                page,
                limit
            );

            // Assert
            Assert.Equal(userId, query.UserId);
            Assert.Equal(role, query.Role);
            Assert.Equal("User", query.ViewAs);
            Assert.Equal(filterUserId, query.FilterUserId);
            Assert.Equal(courtId, query.CourtId);
            Assert.Equal(sportsCenterId, query.SportsCenterId);
            Assert.Equal(status, query.Status);
            Assert.Equal(startDate, query.StartDate);
            Assert.Equal(endDate, query.EndDate);
            Assert.Equal(page, query.Page);
            Assert.Equal(limit, query.Limit);
        }

        [Fact]
        public void Constructor_Should_AcceptNullOptionalParameters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = "User";
            Guid? filterUserId = null;
            Guid? courtId = null;
            Guid? sportsCenterId = null;
            BookingStatus? status = null;
            DateTime? startDate = null;
            DateTime? endDate = null;
            int page = 0;
            int limit = 10;

            // Act
            var query = new GetBookingsQuery(
                userId,
                role,
                null,
                filterUserId,
                courtId,
                sportsCenterId,
                status,
                startDate,
                endDate,
                page,
                limit
            );

            // Assert
            Assert.Equal(userId, query.UserId);
            Assert.Equal(role, query.Role);
            Assert.Null(query.ViewAs);
            Assert.Null(query.FilterUserId);
            Assert.Null(query.CourtId);
            Assert.Null(query.SportsCenterId);
            Assert.Null(query.Status);
            Assert.Null(query.StartDate);
            Assert.Null(query.EndDate);
            Assert.Equal(0, query.Page);
            Assert.Equal(10, query.Limit);
        }

        [Fact]
        public void Constructor_Should_AcceptEmptyUserId_When_RoleIsAdmin()
        {
            // Arrange
            var userId = Guid.Empty;
            var role = "Admin";
            int page = 0;
            int limit = 10;

            // Act
            var query = new GetBookingsQuery(
                userId,
                role,
                "Admin",
                null,
                null,
                null,
                null,
                null,
                null,
                page,
                limit
            );

            // Assert
            Assert.Equal(userId, query.UserId);
            Assert.Equal(role, query.Role);
            Assert.Equal("Admin", query.ViewAs);
        }

        [Theory]
        [InlineData(-1, 10)] // Trang âm
        [InlineData(0, 0)]   // Limit = 0
        [InlineData(0, -1)]  // Limit âm
        public void Constructor_Should_Accept_BoundaryPaginationValues(int page, int limit)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = "User";

            // Act
            var query = new GetBookingsQuery(
                userId,
                role,
                "User",
                null,
                null,
                null,
                null,
                null,
                null,
                page,
                limit
            );

            // Assert
            Assert.Equal(page, query.Page);
            Assert.Equal(limit, query.Limit);
            // Ghi chú: Chúng ta chấp nhận các giá trị biên trong constructor, 
            // nhưng Handler sẽ cần xử lý các trường hợp này
        }
    }
}
using CourtBooking.Application.BookingManagement.Queries.GetBookings;
using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CourtBooking.Domain.Enums;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using CourtBooking.Test.Common;
using static CourtBooking.Test.Common.QueryableExtensions;

namespace CourtBooking.Test.Application.BookingManagement.Queries.GetBookings
{
    public class GetBookingsHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<ISportCenterRepository> _mockSportCenterRepository;
        private readonly GetBookingsHandler _handler;
        private readonly Mock<DbSet<Booking>> _mockBookingsDbSet;
        private readonly Mock<DbSet<Court>> _mockCourtsDbSet;
        private readonly Mock<DbSet<SportCenter>> _mockSportCentersDbSet;

        public GetBookingsHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockSportCenterRepository = new Mock<ISportCenterRepository>();
            _mockBookingsDbSet = new Mock<DbSet<Booking>>();
            _mockCourtsDbSet = new Mock<DbSet<Court>>();
            _mockSportCentersDbSet = new Mock<DbSet<SportCenter>>();

            // Setup the mock DbSets with empty collections initially
            var bookings = new List<Booking>().AsQueryable();
            var courts = new List<Court>().AsQueryable();
            var sportCenters = new List<SportCenter>().AsQueryable();

            // Set up all the necessary interface implementations before accessing Object
            _mockBookingsDbSet.As<IAsyncEnumerable<Booking>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Booking>(bookings.GetEnumerator()));

            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.Provider).Returns(bookings.Provider);
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.Expression).Returns(bookings.Expression);
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.ElementType).Returns(bookings.ElementType);
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.GetEnumerator()).Returns(bookings.GetEnumerator());

            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.Provider).Returns(courts.Provider);
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.Expression).Returns(courts.Expression);
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.ElementType).Returns(courts.ElementType);
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.GetEnumerator()).Returns(courts.GetEnumerator());

            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.Provider).Returns(sportCenters.Provider);
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.Expression).Returns(sportCenters.Expression);
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.ElementType).Returns(sportCenters.ElementType);
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.GetEnumerator()).Returns(sportCenters.GetEnumerator());

            _mockContext.Setup(c => c.Bookings).Returns(_mockBookingsDbSet.Object);
            _mockContext.Setup(c => c.Courts).Returns(_mockCourtsDbSet.Object);
            _mockContext.Setup(c => c.SportCenters).Returns(_mockSportCentersDbSet.Object);

            _handler = new GetBookingsHandler(
                _mockContext.Object,
                _mockBookingRepository.Object,
                _mockSportCenterRepository.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnUserBookings_WhenRoleIsUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetBookingsQuery(
                UserId: userId,
                Role: "User",
                ViewAs: "User",
                FilterUserId: null,
                CourtId: null,
                SportsCenterId: null,
                Status: null,
                StartDate: null,
                EndDate: null,
                Page: 0,
                Limit: 10
            );

            // Create bookings with the specified user ID
            var bookings = new List<Booking>
            {
                CreateBookingWithUserId(UserId.Of(userId)),
                CreateBookingWithUserId(UserId.Of(userId))
            };

            // Setup the bookings in the repository
            _mockBookingRepository.Setup(r => r.GetBookingsAsync(
                It.Is<Guid?>(id => id == userId),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<BookingStatus?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.Is<int>(p => p == 0),
                It.Is<int>(l => l == 10),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            _mockBookingRepository.Setup(r => r.GetBookingsCountAsync(
                It.Is<Guid?>(id => id == userId),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<BookingStatus?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings.Count);

            // Setup the mocked DbSets with test data
            var bookingsQueryable = bookings.AsQueryable();
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.Provider).Returns(bookingsQueryable.Provider);
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.Expression).Returns(bookingsQueryable.Expression);
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.ElementType).Returns(bookingsQueryable.ElementType);
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.GetEnumerator()).Returns(bookingsQueryable.GetEnumerator());

            // Mock the required DbSet Include behavior
            var mockIncludableQueryable = new Mock<IIncludableQueryable<Booking, ICollection<BookingDetail>>>();
            mockIncludableQueryable.Setup(m => m.Provider).Returns(bookingsQueryable.Provider);
            mockIncludableQueryable.Setup(m => m.Expression).Returns(bookingsQueryable.Expression);
            mockIncludableQueryable.Setup(m => m.ElementType).Returns(bookingsQueryable.ElementType);
            mockIncludableQueryable.Setup(m => m.GetEnumerator()).Returns(bookingsQueryable.GetEnumerator());

            // Setup the mockBookingsDbSet to return our mockIncludableQueryable when Include is called
            mockIncludableQueryable.As<IQueryable<Booking>>().Setup(m => m.Provider).Returns(bookingsQueryable.Provider);
            mockIncludableQueryable.As<IQueryable<Booking>>().Setup(m => m.Expression).Returns(bookingsQueryable.Expression);
            mockIncludableQueryable.As<IQueryable<Booking>>().Setup(m => m.ElementType).Returns(bookingsQueryable.ElementType);
            mockIncludableQueryable.As<IQueryable<Booking>>().Setup(m => m.GetEnumerator()).Returns(bookingsQueryable.GetEnumerator());

            // Prepare test court and sport center data
            var courtId = CourtId.Of(Guid.NewGuid());
            var sportCenterId = SportCenterId.Of(Guid.NewGuid());
            var court = CreateCourt(courtId, sportCenterId);
            var sportCenter = CreateSportCenter(sportCenterId, userId);

            // Add court and booking detail data to the bookings
            foreach (var booking in bookings)
            {
                var details = GetBookingDetailsFromBooking(booking);
                foreach (var detail in details)
                {
                    typeof(BookingDetail).GetProperty("CourtId", BindingFlags.Public | BindingFlags.Instance)
                        ?.SetValue(detail, courtId);
                }
            }

            var courtsQueryable = new List<Court> { court }.AsQueryable();
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.Provider).Returns(courtsQueryable.Provider);
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.Expression).Returns(courtsQueryable.Expression);
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.ElementType).Returns(courtsQueryable.ElementType);
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.GetEnumerator()).Returns(courtsQueryable.GetEnumerator());

            var sportCentersQueryable = new List<SportCenter> { sportCenter }.AsQueryable();
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.Provider).Returns(sportCentersQueryable.Provider);
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.Expression).Returns(sportCentersQueryable.Expression);
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.ElementType).Returns(sportCentersQueryable.ElementType);
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.GetEnumerator()).Returns(sportCentersQueryable.GetEnumerator());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Bookings.Count);
        }

        [Fact]
        public async Task Handle_Should_FilterByUserId_WhenAdminWithFilterUserId()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var query = new GetBookingsQuery(
                UserId: adminId,
                Role: "Admin",
                ViewAs: "Admin",
                FilterUserId: targetUserId,
                CourtId: null,
                SportsCenterId: null,
                Status: null,
                StartDate: null,
                EndDate: null,
                Page: 0,
                Limit: 10
            );

            // Create bookings with the target user ID
            var bookings = new List<Booking>
            {
                CreateBookingWithUserId(UserId.Of(targetUserId)),
                CreateBookingWithUserId(UserId.Of(targetUserId))
            };

            // Setup a queryable for the bookings
            var bookingsQueryable = bookings.AsQueryable();
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.Provider).Returns(bookingsQueryable.Provider);
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.Expression).Returns(bookingsQueryable.Expression);
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.ElementType).Returns(bookingsQueryable.ElementType);
            _mockBookingsDbSet.As<IQueryable<Booking>>().Setup(m => m.GetEnumerator()).Returns(bookingsQueryable.GetEnumerator());

            // We don't need this now since we set it up in the constructor
            //_mockBookingsDbSet.As<IAsyncEnumerable<Booking>>()
            //    .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            //    .Returns(new TestAsyncEnumerator<Booking>(bookings.GetEnumerator()));

            // Mock repository to return these bookings when filtering by targetUserId
            _mockBookingRepository.Setup(r => r.GetBookingsAsync(
                It.Is<Guid?>(id => id == targetUserId),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<BookingStatus?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.Is<int>(p => p == 0),
                It.Is<int>(l => l == 10),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            _mockBookingRepository.Setup(r => r.GetBookingsCountAsync(
                It.Is<Guid?>(id => id == targetUserId),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<BookingStatus?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings.Count);

            // Add court and booking detail data to make the mapping work correctly
            var courtId = CourtId.Of(Guid.NewGuid());
            var sportCenterId = SportCenterId.Of(Guid.NewGuid());
            var court = CreateCourt(courtId, sportCenterId);
            var sportCenter = CreateSportCenter(sportCenterId, adminId);

            // Set up court in each booking detail
            foreach (var booking in bookings)
            {
                var details = GetBookingDetailsFromBooking(booking);
                foreach (var detail in details)
                {
                    typeof(BookingDetail).GetProperty("CourtId", BindingFlags.Public | BindingFlags.Instance)
                        ?.SetValue(detail, courtId);
                }
            }

            // Set up Court and SportCenter DbSets
            var courtsQueryable = new List<Court> { court }.AsQueryable();
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.Provider).Returns(courtsQueryable.Provider);
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.Expression).Returns(courtsQueryable.Expression);
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.ElementType).Returns(courtsQueryable.ElementType);
            _mockCourtsDbSet.As<IQueryable<Court>>().Setup(m => m.GetEnumerator()).Returns(courtsQueryable.GetEnumerator());

            var sportCentersQueryable = new List<SportCenter> { sportCenter }.AsQueryable();
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.Provider).Returns(sportCentersQueryable.Provider);
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.Expression).Returns(sportCentersQueryable.Expression);
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.ElementType).Returns(sportCentersQueryable.ElementType);
            _mockSportCentersDbSet.As<IQueryable<SportCenter>>().Setup(m => m.GetEnumerator()).Returns(sportCentersQueryable.GetEnumerator());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Bookings.Count);
            Assert.All(result.Bookings, b => Assert.Equal(targetUserId, b.UserId));
        }

        #region Helper Methods

        private List<BookingDetail> GetBookingDetailsFromBooking(Booking booking)
        {
            // Lấy booking details từ booking để thiết lập mock
            var field = typeof(Booking).GetField("_bookingDetails", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(booking) as List<BookingDetail> ?? new List<BookingDetail>();
            }
            return new List<BookingDetail>();
        }

        private Booking CreateBookingWithUserId(UserId userId)
        {
            var booking = Booking.Create(
                BookingId.Of(Guid.NewGuid()),
                userId,
                DateTime.UtcNow
            );

            var detail = BookingDetail.Create(
                booking.Id,
                CourtId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(11),
                new List<CourtSchedule>()
            );

            var details = new List<BookingDetail> { detail };
            typeof(Booking).GetField("_bookingDetails", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(booking, details);

            return booking;
        }

        private Booking CreateBookingWithStatus(UserId userId, BookingStatus status)
        {
            var booking = CreateBookingWithUserId(userId);
            booking.UpdateStatus(status);
            return booking;
        }

        private Booking CreateBookingWithDateAndUserId(DateTime date, UserId userId)
        {
            var booking = Booking.Create(
                BookingId.Of(Guid.NewGuid()),
                userId,
                date
            );

            var detail = BookingDetail.Create(
                booking.Id,
                CourtId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(date.Hour),
                TimeSpan.FromHours(date.Hour + 1),
                new List<CourtSchedule>()
            );

            var details = new List<BookingDetail> { detail };
            typeof(Booking).GetField("_bookingDetails", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(booking, details);

            return booking;
        }

        private Booking CreateBookingWithCourtAndUserId(CourtId courtId, UserId userId)
        {
            var booking = Booking.Create(
                BookingId.Of(Guid.NewGuid()),
                userId,
                DateTime.UtcNow
            );

            var detail = BookingDetail.Create(
                booking.Id,
                courtId,
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );

            var details = new List<BookingDetail> { detail };
            typeof(Booking).GetField("_bookingDetails", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(booking, details);

            return booking;
        }

        private Court CreateCourt(SportCenterId sportCenterId)
        {
            var courtId = CourtId.Of(Guid.NewGuid());
            return CreateCourt(courtId, sportCenterId);
        }

        private Court CreateCourt(CourtId courtId, SportCenterId sportCenterId)
        {
            return Court.Create(
                courtId,
                CourtName.Of("Test Court"),
                sportCenterId,
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1),
                "Description",
                "[]", // JSON array rỗng cho facilities
                CourtType.Indoor,
                30
            );
        }

        private SportCenter CreateSportCenter(SportCenterId id, Guid ownerId)
        {
            return SportCenter.Create(
                id,
                OwnerId.Of(ownerId),
                "Test SportCenter",
                "Phone",
                new Location("123 Test St", "Test City", "Test District", "Test Commune"),
                new GeoLocation(10.0, 20.0),
                new SportCenterImages("avatar.jpg", new List<string> { "image1.jpg", "image2.jpg" }),
                "Description"
            );
        }

        #endregion Helper Methods
    }
}
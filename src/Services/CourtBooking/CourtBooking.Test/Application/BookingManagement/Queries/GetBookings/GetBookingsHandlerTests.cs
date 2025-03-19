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

namespace CourtBooking.Test.Application.BookingManagement.Queries.GetBookings
{
    public class GetBookingsHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<ISportCenterRepository> _mockSportCenterRepository;
        private readonly Mock<DbSet<Booking>> _mockBookingDbSet;
        private readonly Mock<DbSet<Court>> _mockCourtDbSet;
        private readonly Mock<DbSet<SportCenter>> _mockSportCenterDbSet;
        private readonly GetBookingsHandler _handler;

        public GetBookingsHandlerTests()
        {
            _mockBookingDbSet = new Mock<DbSet<Booking>>();
            _mockCourtDbSet = new Mock<DbSet<Court>>();
            _mockSportCenterDbSet = new Mock<DbSet<SportCenter>>();

            _mockContext = new Mock<IApplicationDbContext>();
            _mockContext.Setup(c => c.Bookings).Returns(_mockBookingDbSet.Object);
            _mockContext.Setup(c => c.Courts).Returns(_mockCourtDbSet.Object);
            _mockContext.Setup(c => c.SportCenters).Returns(_mockSportCenterDbSet.Object);

            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockSportCenterRepository = new Mock<ISportCenterRepository>();

            _handler = new GetBookingsHandler(
                _mockContext.Object,
                _mockBookingRepository.Object,
                _mockSportCenterRepository.Object
            );
        }

        #region Test Cases

        // Instead of ##region Test Cases

        [Fact]
        public async Task Handle_Should_ReturnUserBookings_WhenRoleIsUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetBookingsQuery(
                UserId: userId,
                Role: "User",
                FilterUserId: null,
                CourtId: null,
                SportsCenterId: null,
                Status: null,
                StartDate: null,
                EndDate: null,
                Page: 0,
                Limit: 10
            );

            var bookings = new List<Booking>
            {
                CreateBookingWithUserId(UserId.Of(userId)),
                CreateBookingWithUserId(UserId.Of(userId)),
                CreateBookingWithUserId(UserId.Of(Guid.NewGuid()))
            }.AsQueryable();

            SetupDbContext(bookings);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Bookings.Count);
            Assert.All(result.Bookings, b => Assert.Equal(userId, b.UserId));
        }

        [Fact]
        public async Task Handle_Should_ReturnAllBookings_WhenRoleIsAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetBookingsQuery(
                UserId: userId,
                Role: "Admin",
                FilterUserId: null,
                CourtId: null,
                SportsCenterId: null,
                Status: null,
                StartDate: null,
                EndDate: null,
                Page: 0,
                Limit: 10
            );

            var bookings = new List<Booking>
            {
                CreateBookingWithUserId(UserId.Of(Guid.NewGuid())),
                CreateBookingWithUserId(UserId.Of(Guid.NewGuid())),
                CreateBookingWithUserId(UserId.Of(Guid.NewGuid()))
            }.AsQueryable();

            SetupDbContext(bookings);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Bookings.Count);
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
                FilterUserId: targetUserId,
                CourtId: null,
                SportsCenterId: null,
                Status: null,
                StartDate: null,
                EndDate: null,
                Page: 0,
                Limit: 10
            );

            var bookings = new List<Booking>
            {
                CreateBookingWithUserId(UserId.Of(targetUserId)),
                CreateBookingWithUserId(UserId.Of(targetUserId)),
                CreateBookingWithUserId(UserId.Of(Guid.NewGuid()))
            }.AsQueryable();

            SetupDbContext(bookings);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Bookings.Count);
            Assert.All(result.Bookings, b => Assert.Equal(targetUserId, b.UserId));
        }

        [Fact]
        public async Task Handle_Should_FilterByStatus_WhenStatusProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetBookingsQuery(
                UserId: userId,
                Role: "User",
                FilterUserId: null,
                CourtId: null,
                SportsCenterId: null,
                Status: BookingStatus.Confirmed,
                StartDate: null,
                EndDate: null,
                Page: 0,
                Limit: 10
            );

            var bookings = new List<Booking>
            {
                CreateBookingWithStatus(UserId.Of(userId), BookingStatus.Pending),
                CreateBookingWithStatus(UserId.Of(userId), BookingStatus.Confirmed),
                CreateBookingWithStatus(UserId.Of(userId), BookingStatus.Confirmed),
                CreateBookingWithStatus(UserId.Of(userId), BookingStatus.Cancelled)
            }.AsQueryable();

            SetupDbContext(bookings);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Bookings.Count);
            Assert.All(result.Bookings, b => Assert.Equal("Confirmed", b.Status));
        }

        [Fact]
        public async Task Handle_Should_FilterByDateRange_WhenDatesProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = DateTime.Today.AddDays(-5);
            var endDate = DateTime.Today.AddDays(5);
            var query = new GetBookingsQuery(
                UserId: userId,
                Role: "User",
                FilterUserId: null,
                CourtId: null,
                SportsCenterId: null,
                Status: null,
                StartDate: startDate,
                EndDate: endDate,
                Page: 0,
                Limit: 10
            );

            var bookings = new List<Booking>
            {
                CreateBookingWithDateAndUserId(DateTime.Today.AddDays(-10), UserId.Of(userId)),
                CreateBookingWithDateAndUserId(DateTime.Today, UserId.Of(userId)),
                CreateBookingWithDateAndUserId(DateTime.Today.AddDays(3), UserId.Of(userId)),
                CreateBookingWithDateAndUserId(DateTime.Today.AddDays(10), UserId.Of(userId))
            }.AsQueryable();

            SetupDbContext(bookings);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Bookings.Count);
        }

        [Fact]
        public async Task Handle_Should_ReturnOwnedCenterBookings_WhenRoleIsCourtOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var sportCenterId1 = SportCenterId.Of(Guid.NewGuid());
            var sportCenterId2 = SportCenterId.Of(Guid.NewGuid());

            var query = new GetBookingsQuery(
                UserId: ownerId,
                Role: "CourtOwner",
                FilterUserId: null,
                CourtId: null,
                SportsCenterId: null,
                Status: null,
                StartDate: null,
                EndDate: null,
                Page: 0,
                Limit: 10
            );

            var ownedSportCenters = new List<SportCenter>
            {
                CreateSportCenter(sportCenterId1, ownerId),
                CreateSportCenter(sportCenterId2, ownerId)
            };

            _mockSportCenterRepository.Setup(r => r.GetSportCentersByOwnerIdAsync(
                ownerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ownedSportCenters);

            var court1 = CreateCourt(sportCenterId1);
            var court2 = CreateCourt(sportCenterId2);
            var courtOtherOwner = CreateCourt(SportCenterId.Of(Guid.NewGuid()));

            var courts = new List<Court> { court1, court2, courtOtherOwner }.AsQueryable();
            SetupCourtDbSet(courts);

            var booking1 = CreateBookingWithCourtAndUserId(court1.Id, UserId.Of(Guid.NewGuid()));
            var booking2 = CreateBookingWithCourtAndUserId(court2.Id, UserId.Of(Guid.NewGuid()));
            var booking3 = CreateBookingWithCourtAndUserId(courtOtherOwner.Id, UserId.Of(Guid.NewGuid()));

            var bookings = new List<Booking> { booking1, booking2, booking3 }.AsQueryable();
            SetupBookingDbSet(bookings);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Bookings.Count);
        }

        [Fact]
        public async Task Handle_Should_ApplyPagination_WhenLimitAndPageProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetBookingsQuery(
                UserId: userId,
                Role: "User",
                FilterUserId: null,
                CourtId: null,
                SportsCenterId: null,
                Status: null,
                StartDate: null,
                EndDate: null,
                Page: 1,
                Limit: 2
            );

            var bookings = new List<Booking>();
            for (int i = 0; i < 5; i++)
            {
                bookings.Add(CreateBookingWithUserId(UserId.Of(userId)));
            }

            SetupDbContext(bookings.AsQueryable());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Bookings.Count);
        }

        #endregion Test Cases

        // Instead of ##endregion

        #region Helper Methods

        // Instead of ##region Helper Methods

        private Booking CreateBookingWithUserId(UserId userId)
        {
            var bookingId = BookingId.Of(Guid.NewGuid());
            var booking = Booking.Create(bookingId, userId, DateTime.Today);
            var detail = BookingDetail.Create(
                bookingId,
                CourtId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
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
            var bookingId = BookingId.Of(Guid.NewGuid());
            var booking = Booking.Create(bookingId, userId, date);
            var detail = BookingDetail.Create(
                bookingId,
                CourtId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                new List<CourtSchedule>()
            );
            var details = new List<BookingDetail> { detail };
            typeof(Booking).GetField("_bookingDetails", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(booking, details);
            return booking;
        }

        private Booking CreateBookingWithCourtAndUserId(CourtId courtId, UserId userId)
        {
            var booking = CreateBookingWithUserId(userId);
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
            return Court.Create(
                courtId,
                CourtName.Of("Test Court"),
                sportCenterId,
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromHours(1),
                "Description",
                "Facilities",
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

        private void SetupDbContext(IQueryable<Booking> bookings)
        {
            SetupBookingDbSet(bookings);

            var courts = new List<Court>
            {
                CreateCourt(SportCenterId.Of(Guid.NewGuid())),
                CreateCourt(SportCenterId.Of(Guid.NewGuid()))
            }.AsQueryable();
            SetupCourtDbSet(courts);

            var sportCenters = new List<SportCenter>
            {
                CreateSportCenter(courts.First().SportCenterId, Guid.NewGuid()),
                CreateSportCenter(courts.Last().SportCenterId, Guid.NewGuid())
            }.AsQueryable();
            SetupSportCenterDbSet(sportCenters);
        }

        private void SetupBookingDbSet(IQueryable<Booking> bookings)
        {
            var asyncBookings = bookings.ToList().AsQueryable();
            var asyncProvider = new TestAsyncQueryProvider<Booking>(asyncBookings.Provider);

            _mockBookingDbSet.As<IAsyncEnumerable<Booking>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Booking>(asyncBookings.GetEnumerator()));

            _mockBookingDbSet.As<IQueryable<Booking>>().Setup(m => m.Provider).Returns(asyncProvider);
            _mockBookingDbSet.As<IQueryable<Booking>>().Setup(m => m.Expression).Returns(asyncBookings.Expression);
            _mockBookingDbSet.As<IQueryable<Booking>>().Setup(m => m.ElementType).Returns(asyncBookings.ElementType);
            _mockBookingDbSet.As<IQueryable<Booking>>().Setup(m => m.GetEnumerator()).Returns(asyncBookings.GetEnumerator());

            _mockBookingDbSet.Setup(s => s.Include(It.IsAny<string>())).Returns(_mockBookingDbSet.Object);
        }

        private void SetupCourtDbSet(IQueryable<Court> courts)
        {
            var asyncCourts = courts.ToList().AsQueryable();
            var asyncProvider = new TestAsyncQueryProvider<Court>(asyncCourts.Provider);

            _mockCourtDbSet.As<IAsyncEnumerable<Court>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Court>(asyncCourts.GetEnumerator()));

            _mockCourtDbSet.As<IQueryable<Court>>().Setup(m => m.Provider).Returns(asyncProvider);
            _mockCourtDbSet.As<IQueryable<Court>>().Setup(m => m.Expression).Returns(asyncCourts.Expression);
            _mockCourtDbSet.As<IQueryable<Court>>().Setup(m => m.ElementType).Returns(asyncCourts.ElementType);
            _mockCourtDbSet.As<IQueryable<Court>>().Setup(m => m.GetEnumerator()).Returns(asyncCourts.GetEnumerator());

            _mockCourtDbSet.Setup(s => s.ToDictionaryAsync(
                c => c.Id,
                c => c,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(courts.ToDictionary(c => c.Id, c => c));
        }

        private void SetupSportCenterDbSet(IQueryable<SportCenter> sportCenters)
        {
            var asyncSportCenters = sportCenters.ToList().AsQueryable();
            var asyncProvider = new TestAsyncQueryProvider<SportCenter>(asyncSportCenters.Provider);

            _mockSportCenterDbSet.As<IAsyncEnumerable<SportCenter>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<SportCenter>(asyncSportCenters.GetEnumerator()));

            _mockSportCenterDbSet.As<IQueryable<SportCenter>>().Setup(m => m.Provider).Returns(asyncProvider);
            _mockSportCenterDbSet.As<IQueryable<SportCenter>>().Setup(m => m.Expression).Returns(asyncSportCenters.Expression);
            _mockSportCenterDbSet.As<IQueryable<SportCenter>>().Setup(m => m.ElementType).Returns(asyncSportCenters.ElementType);
            _mockSportCenterDbSet.As<IQueryable<SportCenter>>().Setup(m => m.GetEnumerator()).Returns(asyncSportCenters.GetEnumerator());

            _mockSportCenterDbSet.Setup(s => s.ToDictionaryAsync(
                sc => sc.Id,
                sc => sc.Name,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(sportCenters.ToDictionary(sc => sc.Id, sc => sc.Name));
        }

        #endregion Helper Methods

        #region Async Query Provider Helpers

        public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return _inner.CreateQuery(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                return new TestAsyncEnumerable<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                return Execute<TResult>(expression);
            }
        }

        public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
            {
            }

            public TestAsyncEnumerable(Expression expression) : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return default;
            }
        }

        #endregion Async Query Provider Helpers
    }
}
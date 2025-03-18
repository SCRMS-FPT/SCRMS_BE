using Coach.API.Data.Repositories;
using Coach.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Coach.API.Data.Models;

namespace Coach.API.Tests.Repositories
{
    public class CoachBookingRepositoryTests
    {
        private readonly CoachBookingRepository _repository;
        private readonly CoachDbContext _context;

        public CoachBookingRepositoryTests()
        {
            // Sử dụng cơ sở dữ liệu trong bộ nhớ
            var options = new DbContextOptionsBuilder<CoachDbContext>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Mỗi lần test sử dụng database mới
                          .Options;
            _context = new CoachDbContext(options);
            _repository = new CoachBookingRepository(_context);
        }

        [Fact]
        public async Task AddCoachBookingAsync_ValidBooking_AddsBooking()
        {
            // Arrange
            var booking = new CoachBooking
            {
                Id = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                BookingDate = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11))
            };

            // Act
            await _repository.AddCoachBookingAsync(booking, CancellationToken.None);

            // Assert
            var addedBooking = await _context.CoachBookings.FindAsync(booking.Id);
            Assert.NotNull(addedBooking);
            Assert.Equal(booking.Id, addedBooking.Id);
        }

        [Fact]
        public async Task AddCoachBookingAsync_InvalidBooking_ThrowsException()
        {
            // Arrange
            var booking = new CoachBooking { Id = Guid.NewGuid() }; // Thiếu CoachId

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddCoachBookingAsync(booking, CancellationToken.None));

            // Kiểm tra nếu exception chứa thông báo "CoachId is required"
            Assert.Equal("CoachId is required (Parameter 'CoachId')", exception.Message);

            // Kiểm tra xem không có entry nào được thêm vào DbContext
            var entries = _context.ChangeTracker.Entries<CoachBooking>().ToList();
            Assert.Empty(entries.Where(entry => entry.State == EntityState.Added));
        }

        [Fact]
        public async Task GetCoachBookingByIdAsync_ExistingId_ReturnsBooking()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = new CoachBooking { Id = bookingId, CoachId = Guid.NewGuid() };
            await _context.CoachBookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachBookingByIdAsync(bookingId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookingId, result.Id);
        }

        [Fact]
        public async Task GetCoachBookingByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var bookingId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachBookingByIdAsync(bookingId, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCoachBookingAsync_ExistingBooking_UpdatesBooking()
        {
            // Arrange
            var booking = new CoachBooking
            {
                Id = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                BookingDate = DateOnly.FromDateTime(DateTime.Today)
            };
            await _context.CoachBookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Thay đổi thông tin booking
            booking.BookingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            // Act
            await _repository.UpdateCoachBookingAsync(booking, CancellationToken.None);

            // Assert
            var updatedBooking = await _context.CoachBookings.FindAsync(booking.Id);
            Assert.NotNull(updatedBooking);
            Assert.Equal(booking.BookingDate, updatedBooking.BookingDate);
        }

        [Fact]
        public async Task UpdateCoachBookingAsync_NonExistingBooking_ThrowsException()
        {
            // Arrange
            var booking = new CoachBooking { Id = Guid.NewGuid(), CoachId = Guid.NewGuid() };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateCoachBookingAsync(booking, CancellationToken.None));

            // Verify the exception message (optional, but it helps to understand the failure)
            Assert.Contains("The booking does not exist in the database", exception.Message);
        }

        [Fact]
        public async Task GetCoachBookingsByCoachIdAsync_CoachWithBookings_ReturnsBookings()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var bookings = new List<CoachBooking>
            {
                new CoachBooking { Id = Guid.NewGuid(), CoachId = coachId },
                new CoachBooking { Id = Guid.NewGuid(), CoachId = coachId }
            };

            await _context.CoachBookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCoachBookingsByCoachIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, b => Assert.Equal(coachId, b.CoachId));
        }

        [Fact]
        public async Task GetCoachBookingsByCoachIdAsync_CoachWithoutBookings_ReturnsEmptyList()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCoachBookingsByCoachIdAsync(coachId, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task HasOverlappingCoachBookingAsync_OverlappingBooking_ReturnsTrue()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var bookingDate = DateOnly.FromDateTime(DateTime.Today);
            var startTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10));
            var endTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11));

            var existingBooking = new CoachBooking
            {
                CoachId = coachId,
                BookingDate = bookingDate,
                StartTime = startTime,
                EndTime = endTime
            };
            await _context.CoachBookings.AddAsync(existingBooking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasOverlappingCoachBookingAsync(coachId, bookingDate, startTime, endTime, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasNoOverlappingCoachBookingAsync_NoOverlap_ReturnsFalse()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var bookingDate = DateOnly.FromDateTime(DateTime.Today);
            var startTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(12));
            var endTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(13));

            var existingBooking = new CoachBooking
            {
                CoachId = coachId,
                BookingDate = bookingDate,
                StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11))
            };
            await _context.CoachBookings.AddAsync(existingBooking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasOverlappingCoachBookingAsync(coachId, bookingDate, startTime, endTime, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
    }
}
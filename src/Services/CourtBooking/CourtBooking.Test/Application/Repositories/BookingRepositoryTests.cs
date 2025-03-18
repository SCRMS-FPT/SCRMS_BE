using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Infrastructure.Data;
using CourtBooking.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Application.Repositories
{
    public class BookingRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookingRepository _repository;

        public BookingRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"BookingDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new BookingRepository(_context);
        }

        [Fact]
        public async Task AddBookingAsync_Should_AddBooking_When_BookingIsValid()
        {
            // Arrange
            var bookingId = BookingId.Of(Guid.NewGuid());
            var userId = UserId.Of(Guid.NewGuid());
            var booking = Booking.Create(
                bookingId,
                userId,
                DateTime.Now.AddDays(1),
                "Test booking"
            );

            // Act
            await _repository.AddBookingAsync(booking);
            await _context.SaveChangesAsync();

            // Assert
            var addedBooking = await _context.Bookings.FindAsync(bookingId);
            Assert.NotNull(addedBooking);
            Assert.Equal(bookingId, addedBooking.Id);
            Assert.Equal(userId, addedBooking.UserId);
        }

        [Fact]
        public async Task GetBookingByIdAsync_Should_ReturnBooking_When_BookingExists()
        {
            // Arrange
            var bookingId = BookingId.Of(Guid.NewGuid());
            var userId = UserId.Of(Guid.NewGuid());
            var booking = Booking.Create(
                bookingId,
                userId,
                DateTime.Now.AddDays(1),
                "Test booking"
            );

            await _repository.AddBookingAsync(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBookingByIdAsync(bookingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookingId, result.Id);
            Assert.Equal(userId, result.UserId);
        }

        [Fact]
        public async Task GetBookingByIdAsync_Should_ReturnNull_When_BookingDoesNotExist()
        {
            // Arrange
            var nonExistingId = BookingId.Of(Guid.NewGuid());

            // Act
            var result = await _repository.GetBookingByIdAsync(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateBookingAsync_Should_UpdateBooking_When_BookingExists()
        {
            // Arrange
            var bookingId = BookingId.Of(Guid.NewGuid());
            var userId = UserId.Of(Guid.NewGuid());
            var booking = Booking.Create(
                bookingId,
                userId,
                DateTime.Now.AddDays(1),
                "Original note"
            );

            await _repository.AddBookingAsync(booking);
            await _context.SaveChangesAsync();

            // Update the booking
            booking.UpdateStatus(BookingStatus.Confirmed);
            booking.SetNote("Updated note");

            // Act
            await _repository.UpdateBookingAsync(booking);
            await _context.SaveChangesAsync();

            // Assert
            var updatedBooking = await _context.Bookings.FindAsync(bookingId);
            Assert.NotNull(updatedBooking);
            Assert.Equal(BookingStatus.Confirmed, updatedBooking.Status);
            Assert.Equal("Updated note", updatedBooking.Note);
        }

        [Fact]
        public async Task GetBookingsInDateRangeForCourtAsync_Should_ReturnBookings_WithinDateRange()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = UserId.Of(Guid.NewGuid());

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var dayAfterTomorrow = today.AddDays(2);

            // Booking for today
            var booking1 = Booking.Create(
                BookingId.Of(Guid.NewGuid()),
                userId,
                today,
                "Today booking"
            );
            booking1.AddBookingDetail(CourtId.Of(courtId), TimeSpan.FromHours(10), TimeSpan.FromHours(12),
                new List<CourtSchedule>(), 0);

            // Booking for tomorrow
            var booking2 = Booking.Create(
                BookingId.Of(Guid.NewGuid()),
                userId,
                tomorrow,
                "Tomorrow booking"
            );
            booking2.AddBookingDetail(CourtId.Of(courtId), TimeSpan.FromHours(14), TimeSpan.FromHours(16),
                new List<CourtSchedule>(), 0);

            // Booking for day after tomorrow
            var booking3 = Booking.Create(
                BookingId.Of(Guid.NewGuid()),
                userId,
                dayAfterTomorrow,
                "Day after tomorrow booking"
            );
            booking3.AddBookingDetail(CourtId.Of(courtId), TimeSpan.FromHours(9), TimeSpan.FromHours(11),
                new List<CourtSchedule>(), 0);

            await _repository.AddBookingAsync(booking1);
            await _repository.AddBookingAsync(booking2);
            await _repository.AddBookingAsync(booking3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBookingsInDateRangeForCourtAsync(
                courtId,
                today,
                tomorrow
            );

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, b => b.BookingDate == today);
            Assert.Contains(result, b => b.BookingDate == tomorrow);
        }

        [Fact]
        public async Task GetBookingDetailsAsync_Should_ReturnDetails_WhenBookingHasDetails()
        {
            // Arrange
            var bookingId = BookingId.Of(Guid.NewGuid());
            var userId = UserId.Of(Guid.NewGuid());
            var courtId = CourtId.Of(Guid.NewGuid());

            var booking = Booking.Create(
                bookingId,
                userId,
                DateTime.Now.AddDays(1),
                "Booking with details"
            );

            booking.AddBookingDetail(courtId, TimeSpan.FromHours(10), TimeSpan.FromHours(12),
                new List<CourtSchedule>(), 0);

            await _repository.AddBookingAsync(booking);
            await _context.SaveChangesAsync();

            // Act
            var details = await _repository.GetBookingDetailsAsync(bookingId);

            // Assert
            Assert.Single(details);
            Assert.Equal(courtId, details.First().CourtId);
            Assert.Equal(TimeSpan.FromHours(10), details.First().StartTime);
            Assert.Equal(TimeSpan.FromHours(12), details.First().EndTime);
        }
    }
}
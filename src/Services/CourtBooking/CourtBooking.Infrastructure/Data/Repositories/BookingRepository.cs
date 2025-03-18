﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Infrastructure.Data.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Booking>> GetBookingsAsync(
            Guid? userId,
            Guid? courtId,
            Guid? sportsCenterId,
            BookingStatus? status,
            DateTime? startDate,
            DateTime? endDate,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = _context.Bookings
                .Include(b => b.BookingDetails)
                .AsQueryable();

            if (userId.HasValue) query = query.Where(b => b.UserId.Value == userId.Value);
            if (courtId.HasValue) query = query.Where(b => b.BookingDetails.Any(d => d.CourtId.Value == courtId.Value));
            if (sportsCenterId.HasValue)
            {
                // Lọc theo SportsCenterId bằng cách sử dụng CourtId để tra cứu trong bảng Courts
                query = query.Where(b => b.BookingDetails.Any(d =>
                    _context.Courts.Any(c => c.Id.Value == d.CourtId.Value && c.SportCenterId.Value == sportsCenterId.Value)));
            }
            if (status.HasValue) query = query.Where(b => b.Status == status.Value);
            if (startDate.HasValue) query = query.Where(b => b.BookingDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(b => b.BookingDate <= endDate.Value);

            return await query
                .OrderBy(b => b.BookingDate)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetBookingsCountAsync(
            Guid? userId,
            Guid? courtId,
            Guid? sportsCenterId,
            BookingStatus? status,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken)
        {
            var query = _context.Bookings.AsQueryable();

            if (userId.HasValue) query = query.Where(b => b.UserId.Value == userId.Value);
            if (courtId.HasValue) query = query.Where(b => b.BookingDetails.Any(d => d.CourtId.Value == courtId.Value));
            if (sportsCenterId.HasValue)
            {
                query = query.Where(b => b.BookingDetails.Any(d =>
                    _context.Courts.Any(c => c.Id.Value == d.CourtId.Value && c.SportCenterId.Value == sportsCenterId.Value)));
            }
            if (status.HasValue) query = query.Where(b => b.Status == status.Value);
            if (startDate.HasValue) query = query.Where(b => b.BookingDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(b => b.BookingDate <= endDate.Value);

            return await query.CountAsync(cancellationToken);
        }

        public async Task AddBookingAsync(Booking booking, CancellationToken cancellationToken)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<Booking> GetBookingsQuery()
        {
            return _context.Bookings.AsQueryable();
        }

        public async Task<Booking> GetBookingByIdAsync(BookingId bookingId, CancellationToken cancellationToken)
        {
            return await _context.Bookings
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
        }

        public async Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteBookingAsync(BookingId bookingId, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings.FindAsync(new object[] { bookingId.Value }, cancellationToken);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<BookingDetail>> GetBookingDetailsAsync(BookingId bookingId, CancellationToken cancellationToken)
        {
            return await _context.BookingDetails
                .Where(d => d.BookingId == bookingId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetBookingsInDateRangeForCourtAsync(Guid courtId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var startDateOnly = DateOnly.FromDateTime(startDate);
            var endDateOnly = DateOnly.FromDateTime(endDate);

            return await _context.Bookings
                .Include(b => b.BookingDetails)
                .Where(b => b.BookingDate >= startDate.Date &&
                           b.BookingDate <= endDate.Date &&
                           b.Status != Domain.Enums.BookingStatus.Cancelled &&
                           b.BookingDetails.Any(bd => bd.CourtId == CourtId.Of(courtId)))
                .ToListAsync(cancellationToken);
        }
    }
}
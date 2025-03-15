using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.Data.Repositories
{
    public interface IBookingRepository
    {
        Task AddBookingAsync(Booking booking, CancellationToken cancellationToken);

        Task<Booking> GetBookingByIdAsync(BookingId bookingId, CancellationToken cancellationToken);

        IQueryable<Booking> GetBookingsQuery();

        Task<List<Booking>> GetBookingsAsync(
            Guid? userId,
            Guid? courtId,
            Guid? sportsCenterId,
            BookingStatus? status,
            DateTime? startDate,
            DateTime? endDate,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken);

        Task<int> GetBookingsCountAsync(
            Guid? userId,
            Guid? courtId,
            Guid? sportsCenterId,
            BookingStatus? status,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken);

        Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken);

        Task DeleteBookingAsync(BookingId bookingId, CancellationToken cancellationToken);

        Task<List<BookingDetail>> GetBookingDetailsAsync(BookingId bookingId, CancellationToken cancellationToken);
    }
}
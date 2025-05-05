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

        /// <summary>
        /// Lấy danh sách booking trong khoảng thời gian cho một sân cụ thể
        /// </summary>
        Task<IEnumerable<Booking>> GetBookingsInDateRangeForCourtAsync(Guid courtId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);

        /// <summary>
        /// Gets active bookings for a court with specified statuses and booking date after the given date
        /// </summary>
        /// <param name="courtId">The ID of the court</param>
        /// <param name="statuses">The booking statuses to include</param>
        /// <param name="afterDate">The date after which to check bookings</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of active bookings</returns>
        Task<List<Booking>> GetActiveBookingsForCourtAsync(
            CourtId courtId,
            BookingStatus[] statuses,
            DateTime afterDate,
            CancellationToken cancellationToken);
    }
}
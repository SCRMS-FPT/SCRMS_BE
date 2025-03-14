using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.Data.Repositories
{
    public interface IBookingRepository
    {
        Task AddBookingAsync(Booking booking, CancellationToken cancellationToken);

        Task<Booking> GetBookingByIdAsync(BookingId bookingId, CancellationToken cancellationToken);
    }
}
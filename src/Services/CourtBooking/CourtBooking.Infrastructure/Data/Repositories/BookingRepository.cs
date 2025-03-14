using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourtBooking.Application.Data.Repositories;

namespace CourtBooking.Application.Data.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly IApplicationDbContext _context;

        public BookingRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddBookingAsync(Booking booking, CancellationToken cancellationToken)
        {
            await _context.Bookings.AddAsync(booking, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Booking> GetBookingByIdAsync(BookingId bookingId, CancellationToken cancellationToken)
        {
            return await _context.Bookings
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
        }
    }
}
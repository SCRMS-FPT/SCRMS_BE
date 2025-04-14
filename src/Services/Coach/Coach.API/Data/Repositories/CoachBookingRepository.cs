using Coach.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Data.Repositories
{
    public class CoachBookingRepository : ICoachBookingRepository
    {
        private readonly CoachDbContext _context;

        public CoachBookingRepository(CoachDbContext context)
        {
            _context = context;
        }

        public async Task AddCoachBookingAsync(CoachBooking booking, CancellationToken cancellationToken)
        {
            if (booking.CoachId == Guid.Empty)
            {
                throw new ArgumentException("CoachId is required", nameof(booking.CoachId));
            }
            await _context.CoachBookings.AddAsync(booking, cancellationToken);
        }
        public IQueryable<CoachBooking> GetCoachBookingsByUserIdQueryable(Guid userId)
        {
            return _context.CoachBookings.Where(b => b.UserId == userId);
        }
        public async Task<CoachBooking?> GetCoachBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            return await _context.CoachBookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
        }

        public IQueryable<CoachBooking> GetCoachBookingsByCoachIdQueryable(Guid coachId)
        {
            return _context.CoachBookings.Where(b => b.CoachId == coachId).AsQueryable();
        }

        public async Task UpdateCoachBookingAsync(CoachBooking booking, CancellationToken cancellationToken)
        {
            // Check if the booking exists in the database
            var existingBooking = await _context.CoachBookings
                .FirstOrDefaultAsync(b => b.Id == booking.Id, cancellationToken);

            if (existingBooking == null)
            {
                throw new DbUpdateConcurrencyException("The booking does not exist in the database.");
            }

            // Update the booking if it exists
            _context.CoachBookings.Update(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<CoachBooking>> GetCoachBookingsByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachBookings.Where(b => b.CoachId == coachId).ToListAsync(cancellationToken);
        }

        public async Task<bool> HasOverlappingCoachBookingAsync(Guid coachId, DateOnly bookingDate, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken)
        {
            return await _context.CoachBookings.AnyAsync(b =>
                b.CoachId == coachId &&
                b.BookingDate == bookingDate &&
                b.StartTime < endTime &&
                b.EndTime > startTime, cancellationToken);
        }

        public async Task<List<CoachBooking>> GetCoachBookingsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.CoachBookings.Where(b => b.UserId == userId).ToListAsync(cancellationToken);
        }
    }
}
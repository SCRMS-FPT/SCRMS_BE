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
            await _context.CoachBookings.AddAsync(booking, cancellationToken);
        }

        public async Task<CoachBooking?> GetCoachBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            return await _context.CoachBookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
        }

        public async Task UpdateCoachBookingAsync(CoachBooking booking, CancellationToken cancellationToken)
        {
            _context.CoachBookings.Update(booking);
            await Task.CompletedTask;
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
    }
}
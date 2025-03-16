using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourtBooking.Application.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Application.Data.Repositories
{
    public class CourtScheduleRepository : ICourtScheduleRepository
    {
        private readonly IApplicationDbContext _context;

        public CourtScheduleRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCourtScheduleAsync(CourtSchedule courtSchedule, CancellationToken cancellationToken)
        {
            await _context.CourtSchedules.AddAsync(courtSchedule, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<CourtSchedule> GetCourtScheduleByIdAsync(CourtScheduleId courtScheduleId, CancellationToken cancellationToken)
        {
            return await _context.CourtSchedules.FindAsync(new object[] { courtScheduleId }, cancellationToken);
        }

        public async Task UpdateCourtScheduleAsync(CourtSchedule courtSchedule, CancellationToken cancellationToken)
        {
            _context.CourtSchedules.Update(courtSchedule);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteCourtScheduleAsync(CourtScheduleId courtScheduleId, CancellationToken cancellationToken)
        {
            var courtSchedule = await _context.CourtSchedules.FindAsync(new object[] { courtScheduleId }, cancellationToken);
            if (courtSchedule != null)
            {
                _context.CourtSchedules.Remove(courtSchedule);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<CourtSchedule>> GetSchedulesByCourtIdAsync(CourtId courtId, CancellationToken cancellationToken)
        {
            return await _context.CourtSchedules
                .Where(cs => cs.CourtId == courtId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CourtSchedule>> GetSchedulesByCourt(CourtId courtId, CancellationToken cancellationToken)
        {
            return await _context.CourtSchedules
                .Where(cs => cs.CourtId == courtId)
                .ToListAsync(cancellationToken);
        }
    }
}
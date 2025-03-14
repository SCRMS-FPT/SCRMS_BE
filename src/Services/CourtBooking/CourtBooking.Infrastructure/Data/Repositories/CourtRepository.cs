using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CourtBooking.Application.Data.Repositories;
using System.Threading.Tasks;

namespace CourtBooking.Application.Data.Repositories
{
    public class CourtRepository : ICourtRepository
    {
        private readonly IApplicationDbContext _context;

        public CourtRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCourtAsync(Court court, CancellationToken cancellationToken)
        {
            await _context.Courts.AddAsync(court, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Court> GetCourtByIdAsync(CourtId courtId, CancellationToken cancellationToken)
        {
            return await _context.Courts
                .Include(c => c.CourtSchedules)
                .FirstOrDefaultAsync(c => c.Id == courtId, cancellationToken);
        }

        public async Task UpdateCourtAsync(Court court, CancellationToken cancellationToken)
        {
            _context.Courts.Update(court);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteCourtAsync(CourtId courtId, CancellationToken cancellationToken)
        {
            var court = await _context.Courts.FindAsync(new object[] { courtId }, cancellationToken);
            if (court != null)
            {
                _context.Courts.Remove(court);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<Court>> GetAllCourtsOfSportCenterAsync(SportCenterId sportCenterId, CancellationToken cancellationToken)
        {
            return await _context.Courts
                .Where(c => c.SportCenterId == sportCenterId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Court>> GetPaginatedCourtsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            return await _context.Courts
                .OrderBy(c => c.CourtName.Value)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<long> GetTotalCourtCountAsync(CancellationToken cancellationToken)
        {
            return await _context.Courts.LongCountAsync(cancellationToken);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourtBooking.Application.Data.Repositories;

namespace CourtBooking.Application.Data.Repositories
{
    public class SportCenterRepository : ISportCenterRepository
    {
        private readonly IApplicationDbContext _context;

        public SportCenterRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddSportCenterAsync(SportCenter sportCenter, CancellationToken cancellationToken)
        {
            await _context.SportCenters.AddAsync(sportCenter, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<SportCenter> GetSportCenterByIdAsync(SportCenterId sportCenterId, CancellationToken cancellationToken)
        {
            return await _context.SportCenters
                .Include(sc => sc.Courts)
                .FirstOrDefaultAsync(sc => sc.Id == sportCenterId, cancellationToken);
        }

        public async Task UpdateSportCenterAsync(SportCenter sportCenter, CancellationToken cancellationToken)
        {
            _context.SportCenters.Update(sportCenter);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<SportCenter>> GetPaginatedSportCentersAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            return await _context.SportCenters
                .OrderBy(sc => sc.Name)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .Include(sc => sc.Courts)
                .ToListAsync(cancellationToken);
        }

        public async Task<long> GetTotalSportCenterCountAsync(CancellationToken cancellationToken)
        {
            return await _context.SportCenters.LongCountAsync(cancellationToken);
        }
    }
}
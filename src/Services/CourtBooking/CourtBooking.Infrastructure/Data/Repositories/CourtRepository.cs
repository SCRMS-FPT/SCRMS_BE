using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CourtBooking.Application.Data.Repositories;
using System.Threading.Tasks;
using CourtBooking.Application.Exceptions;
using BuildingBlocks.Exceptions;

namespace CourtBooking.Application.Data.Repositories
{
    public class CourtRepository : ICourtRepository
    {
        private readonly IApplicationDbContext _context;
        private readonly ISportCenterRepository _sportCenterRepository;

        public CourtRepository(IApplicationDbContext context, ISportCenterRepository sportCenterRepository)
        {
            _context = context;
            _sportCenterRepository = sportCenterRepository;
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
        public async Task<List<Court>> GetCourtsBySportCenterIdsAsync(List<SportCenterId> sportCenterIds, CancellationToken cancellationToken)
        {
            return await _context.Courts
                .Where(c => sportCenterIds.Contains(c.SportCenterId))
                .ToListAsync(cancellationToken);
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

        public async Task<Court?> GetCourtByIdAsync(Guid courtId, CancellationToken cancellationToken = default)
        {
            // Giả sử Court.Id là kiểu CourtId với property Value (Guid)
            return await _context.Courts
                .FirstOrDefaultAsync(c => c.Id == CourtId.Of(courtId), cancellationToken);
        }

        public async Task<bool> IsOwnedByUserAsync(Guid courtId, Guid userId, CancellationToken cancellationToken = default)
        {
            var court = await GetCourtByIdAsync(courtId, cancellationToken);
            if (court == null)
                return false;

            // Lấy SportCenter chứa Court (giả sử Court có property SportCenterId với kiểu SportCenterId)
            var sportCenter = await _context.SportCenters
                .FirstOrDefaultAsync(sc => sc.Id == court.SportCenterId, cancellationToken);
            return sportCenter != null && sportCenter.OwnerId == OwnerId.Of(userId);
        }

        public async Task<Guid> GetSportCenterIdAsync(CourtId courtId, CancellationToken cancellationToken = default)
        {
            var court = await GetCourtByIdAsync(courtId, cancellationToken);
            if (court == null)
                throw new NotFoundException($"Không tìm thấy sân với ID {courtId.Value}");

            return court.SportCenterId.Value;
        }

        public async Task<List<Court>> GetCourtsBySportCenterIdAsync(SportCenterId sportCenterId, CancellationToken cancellationToken)
        {
            return await _context.Courts
                .Where(c => c.SportCenterId == sportCenterId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Court>> GetAllCourtsAsync(CancellationToken cancellationToken)
        {
            return await _context.Courts
                .OrderBy(c => c.CourtName.Value)
                .ToListAsync(cancellationToken);
        }
    }
}
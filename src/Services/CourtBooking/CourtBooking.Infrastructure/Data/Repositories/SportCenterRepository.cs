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

        public async Task<List<SportCenter>> GetFilteredPaginatedSportCentersAsync(
       int pageIndex,
       int pageSize,
       string? city,
       string? name,
       CancellationToken cancellationToken)
        {
            var query = _context.SportCenters.AsQueryable();

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(sc => sc.Address.City.ToLower() == city.ToLower());
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(sc => sc.Name.ToLower().Contains(name.ToLower()));
            }

            return await query
                .OrderBy(sc => sc.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<long> GetFilteredSportCenterCountAsync(string? city, string? name, CancellationToken cancellationToken)
        {
            var query = _context.SportCenters.AsQueryable();

            query = query.Where(sc => !sc.IsDeleted);

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(sc => sc.Address.City.ToLower() == city.ToLower());
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(sc => sc.Name.ToLower().Contains(name.ToLower()));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<List<SportCenter>> GetSportCentersByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
        {
            return await _context.SportCenters
                .Where(sc => sc.OwnerId == OwnerId.Of(ownerId))
                .ToListAsync(cancellationToken);
        }

        public async Task<SportCenter?> GetSportCenterByIdAsync(Guid sportCenterId, CancellationToken cancellationToken = default)
        {
            // Giả sử SportCenter.Id là kiểu SportCenterId với property Value (Guid)
            return await _context.SportCenters
                .Include(sc => sc.Courts)
                .FirstOrDefaultAsync(sc => sc.Id == SportCenterId.Of(sportCenterId), cancellationToken);
        }

        public async Task<bool> IsOwnedByUserAsync(Guid sportCenterId, Guid userId, CancellationToken cancellationToken = default)
        {
            var sportCenter = await GetSportCenterByIdAsync(sportCenterId, cancellationToken);
            // So sánh dựa trên property Value của OwnerId (vì OwnerId là wrapper của Guid)
            return sportCenter != null && sportCenter.OwnerId == OwnerId.Of(userId);
        }

        public async Task DeleteSportCenterAsync(SportCenterId sportCenterId, CancellationToken cancellationToken)
        {
            var sportCenter = await _context.SportCenters
                .FirstOrDefaultAsync(sc => sc.Id == sportCenterId, cancellationToken);

            if (sportCenter != null)
            {
                _context.SportCenters.Remove(sportCenter);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SoftDeleteSportCenterAsync(SportCenterId sportCenterId, CancellationToken cancellationToken)
        {
            var sportCenter = await _context.SportCenters
                .FirstOrDefaultAsync(sc => sc.Id == sportCenterId, cancellationToken);

            if (sportCenter != null)
            {
                sportCenter.SetIsDeleted(true);
                sportCenter.SetLastModified(DateTime.UtcNow);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RestoreSportCenterAsync(SportCenterId sportCenterId, CancellationToken cancellationToken)
        {
            var sportCenter = await _context.SportCenters
                .FirstOrDefaultAsync(sc => sc.Id == sportCenterId, cancellationToken);

            if (sportCenter != null)
            {
                sportCenter.SetIsDeleted(false);
                sportCenter.SetLastModified(DateTime.UtcNow);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
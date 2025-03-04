using Microsoft.EntityFrameworkCore;

namespace Coach.API.Data.Repositories
{
    public class CoachPackageRepository : ICoachPackageRepository
    {
        private readonly CoachDbContext _context;

        public CoachPackageRepository(CoachDbContext context)
        {
            _context = context;
        }

        public async Task AddCoachPackageAsync(CoachPackage package, CancellationToken cancellationToken)
        {
            await _context.CoachPackages.AddAsync(package, cancellationToken);
        }

        public async Task<CoachPackage?> GetCoachPackageByIdAsync(Guid packageId, CancellationToken cancellationToken)
        {
            return await _context.CoachPackages.FirstOrDefaultAsync(p => p.Id == packageId, cancellationToken);
        }

        public async Task UpdateCoachPackageAsync(CoachPackage package, CancellationToken cancellationToken)
        {
            _context.CoachPackages.Update(package);
            await Task.CompletedTask;
        }

        public async Task<List<CoachPackage>> GetCoachPackagesByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachPackages.Where(p => p.CoachId == coachId).ToListAsync(cancellationToken);
        }
    }
}
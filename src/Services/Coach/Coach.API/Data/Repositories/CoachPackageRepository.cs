using Coach.API.Data.Models;
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
            var errorMessages = new List<string>();

            if (package.CoachId == Guid.Empty)
            {
                errorMessages.Add("CoachId is required");
            }

            if (string.IsNullOrEmpty(package.Name))
            {
                errorMessages.Add("Name is required");
            }

            if (errorMessages.Any())
            {
                throw new ArgumentException(string.Join(", ", errorMessages));
            }

            await _context.CoachPackages.AddAsync(package, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<List<CoachPackage>> GetActivePackagesByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachPackages
                .Where(p => p.CoachId == coachId && p.Status == "active")
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CoachPackage>> GetAllPackagesByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachPackages
                .Where(p => p.CoachId == coachId)
                .ToListAsync(cancellationToken);
        }
        public async Task<CoachPackage?> GetCoachPackageByIdAsync(Guid packageId, CancellationToken cancellationToken)
        {
            return await _context.CoachPackages.FirstOrDefaultAsync(p => p.Id == packageId, cancellationToken);
        }

        public async Task UpdateCoachPackageAsync(CoachPackage package, CancellationToken cancellationToken)
        {
            var existingPackage = await _context.CoachPackages
                .FirstOrDefaultAsync(p => p.Id == package.Id, cancellationToken);

            if (existingPackage == null)
            {
                throw new DbUpdateConcurrencyException("The coach package does not exist.");
            }

            // If the package exists, update it
            _context.CoachPackages.Update(package);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<CoachPackage>> GetCoachPackagesByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachPackages.Where(p => p.CoachId == coachId).ToListAsync(cancellationToken);
        }
    }
}
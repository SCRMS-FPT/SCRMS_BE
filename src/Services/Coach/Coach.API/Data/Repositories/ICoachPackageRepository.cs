using Coach.API.Data.Models;

namespace Coach.API.Data.Repositories
{
    public interface ICoachPackageRepository
    {
        Task AddCoachPackageAsync(CoachPackage package, CancellationToken cancellationToken);

        Task<CoachPackage?> GetCoachPackageByIdAsync(Guid packageId, CancellationToken cancellationToken);

        Task UpdateCoachPackageAsync(CoachPackage package, CancellationToken cancellationToken);
        Task<List<CoachPackage>> GetActivePackagesByCoachIdAsync(Guid coachId, CancellationToken cancellationToken);
        Task<List<CoachPackage>> GetAllPackagesByCoachIdAsync(Guid coachId, CancellationToken cancellationToken);
        Task<List<CoachPackage>> GetCoachPackagesByCoachIdAsync(Guid coachId, CancellationToken cancellationToken);
    }
}
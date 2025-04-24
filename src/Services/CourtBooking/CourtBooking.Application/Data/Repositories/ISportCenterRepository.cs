using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.Data.Repositories
{
    public interface ISportCenterRepository
    {
        Task AddSportCenterAsync(SportCenter sportCenter, CancellationToken cancellationToken);

        Task<SportCenter> GetSportCenterByIdAsync(SportCenterId sportCenterId, CancellationToken cancellationToken);

        Task UpdateSportCenterAsync(SportCenter sportCenter, CancellationToken cancellationToken);

        Task<List<SportCenter>> GetPaginatedSportCentersAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);

        Task<List<SportCenter>> GetSportCentersByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);

        Task<long> GetTotalSportCenterCountAsync(CancellationToken cancellationToken);

        Task<List<SportCenter>> GetFilteredPaginatedSportCentersAsync(
               int pageIndex,
               int pageSize,
               string? city,
               string? name,
               CancellationToken cancellationToken);

        Task<long> GetFilteredSportCenterCountAsync(string? city, string? name, CancellationToken cancellationToken);

        Task<SportCenter?> GetSportCenterByIdAsync(Guid sportCenterId, CancellationToken cancellationToken = default);

        Task<bool> IsOwnedByUserAsync(Guid sportCenterId, Guid userId, CancellationToken cancellationToken = default);

        Task DeleteSportCenterAsync(SportCenterId sportCenterId, CancellationToken cancellationToken);

        Task SoftDeleteSportCenterAsync(SportCenterId sportCenterId, CancellationToken cancellationToken);

        Task RestoreSportCenterAsync(SportCenterId sportCenterId, CancellationToken cancellationToken);
    }
}
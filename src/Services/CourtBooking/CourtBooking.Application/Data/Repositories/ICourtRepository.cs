using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.Data.Repositories
{
    public interface ICourtRepository
    {
        Task AddCourtAsync(Court court, CancellationToken cancellationToken);

        Task<Court> GetCourtByIdAsync(CourtId courtId, CancellationToken cancellationToken);

        Task UpdateCourtAsync(Court court, CancellationToken cancellationToken);

        Task DeleteCourtAsync(CourtId courtId, CancellationToken cancellationToken);

        Task<List<Court>> GetAllCourtsOfSportCenterAsync(SportCenterId sportCenterId, CancellationToken cancellationToken);

        Task<List<Court>> GetPaginatedCourtsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);

        // Add this new method to get all courts without pagination
        Task<List<Court>> GetAllCourtsAsync(CancellationToken cancellationToken);

        Task<List<Court>> GetCourtsBySportCenterIdsAsync(List<SportCenterId> sportCenterIds, CancellationToken cancellationToken);
        Task<long> GetTotalCourtCountAsync(CancellationToken cancellationToken);

        Task<Court?> GetCourtByIdAsync(Guid courtId, CancellationToken cancellationToken = default);

        Task<bool> IsOwnedByUserAsync(Guid courtId, Guid userId, CancellationToken cancellationToken = default);

        Task<Guid> GetSportCenterIdAsync(CourtId courtId, CancellationToken cancellationToken = default);

        Task<List<Court>> GetCourtsBySportCenterIdAsync(SportCenterId sportCenterId, CancellationToken cancellationToken);
    }
}
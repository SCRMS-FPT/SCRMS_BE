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

        Task<long> GetTotalSportCenterCountAsync(CancellationToken cancellationToken);
    }
}
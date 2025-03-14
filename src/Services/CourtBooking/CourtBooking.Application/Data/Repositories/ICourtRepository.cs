using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        Task<long> GetTotalCourtCountAsync(CancellationToken cancellationToken);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.Data.Repositories
{
    public interface ISportRepository
    {
        Task AddSportAsync(Sport sport, CancellationToken cancellationToken);

        Task<Sport> GetSportByIdAsync(SportId sportId, CancellationToken cancellationToken);

        Task UpdateSportAsync(Sport sport, CancellationToken cancellationToken);

        Task DeleteSportAsync(SportId sportId, CancellationToken cancellationToken);

        Task<List<Sport>> GetAllSportsAsync(CancellationToken cancellationToken);

        Task<bool> IsSportInUseAsync(SportId sportId, CancellationToken cancellationToken);

        Task<List<Sport>> GetSportsByIdsAsync(List<SportId> sportIds, CancellationToken cancellationToken);

        Task<Sport> GetByIdAsync(SportId sportId, CancellationToken cancellationToken);
        Task<Sport> GetByName(string Name, CancellationToken cancellationToken);
    }
}
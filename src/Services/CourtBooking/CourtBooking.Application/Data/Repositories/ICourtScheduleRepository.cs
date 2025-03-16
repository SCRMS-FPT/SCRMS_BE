using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.Data.Repositories
{
    public interface ICourtScheduleRepository
    {
        Task AddCourtScheduleAsync(CourtSchedule courtSchedule, CancellationToken cancellationToken);

        Task<CourtSchedule> GetCourtScheduleByIdAsync(CourtScheduleId courtScheduleId, CancellationToken cancellationToken);

        Task UpdateCourtScheduleAsync(CourtSchedule courtSchedule, CancellationToken cancellationToken);

        Task DeleteCourtScheduleAsync(CourtScheduleId courtScheduleId, CancellationToken cancellationToken);

        Task<List<CourtSchedule>> GetSchedulesByCourtIdAsync(CourtId courtId, CancellationToken cancellationToken);

        /// <summary>
        /// Lấy tất cả lịch trình của một sân
        /// </summary>
        Task<IEnumerable<CourtSchedule>> GetSchedulesByCourt(CourtId courtId, CancellationToken cancellationToken);
    }
}
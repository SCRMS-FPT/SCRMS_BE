using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Data.Repositories;

namespace Identity.Application.Identity.Queries.DashboardStats
{
    public class DashboardStatsQueryHandler : IRequestHandler<DashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IStatsRepository _statsRepository;

        public DashboardStatsQueryHandler(IStatsRepository statsRepository)
        {
            _statsRepository = statsRepository;
        }

        public async Task<DashboardStatsDto> Handle(DashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var stats = await _statsRepository.GetStatsAsync();
            return stats;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Data.Repositories
{
    public interface IStatsRepository
    {
        Task<DashboardStatsDto> GetStatsAsync();
    }
}
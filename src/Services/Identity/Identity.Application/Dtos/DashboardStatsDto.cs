using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Dtos
{
    public record DashboardStatsDto
    {
        public int TotalUsers { get; init; }
        public int TotalCourtOwners { get; init; }
        public int TotalCoaches { get; init; }
        public int TotalCourts { get; init; }
        public int TotalBookings { get; init; }
        public int TotalReviews { get; init; }
        public decimal TotalServicePackageRevenue { get; init; }
        public DateRangeDto DateRange { get; init; }
    }

    public record DateRangeDto(DateTime? StartDate, DateTime? EndDate);
}
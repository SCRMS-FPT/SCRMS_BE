using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Data.Models;

namespace Payment.API.Features.CoachRevenueReport
{
    public class GetCoachRevenueReportHandler : IRequestHandler<GetCoachRevenueReportQuery, CoachRevenueReportDto>
    {
        private readonly PaymentDbContext _context;

        public GetCoachRevenueReportHandler(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<CoachRevenueReportDto> Handle(GetCoachRevenueReportQuery request, CancellationToken cancellationToken)
        {
            // Parse date parameters
            DateTime? startDate = null, endDate = null;
            if (!string.IsNullOrEmpty(request.StartDate) && DateTime.TryParse(request.StartDate, out var parsedStart))
            {
                startDate = DateTime.SpecifyKind(parsedStart, DateTimeKind.Utc);
            }
            if (!string.IsNullOrEmpty(request.EndDate) && DateTime.TryParse(request.EndDate, out var parsedEnd))
            {
                endDate = DateTime.SpecifyKind(parsedEnd.AddDays(1).AddSeconds(-1), DateTimeKind.Utc); // End of day
            }

            // Query transactions for coach revenue
            var query = _context.WalletTransactions.AsQueryable()
                .Where(t => t.UserId == request.CoachId &&
                           (t.TransactionType == "PackageRevenue" || t.TransactionType == "CoachBookingRevenue"));

            if (startDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= endDate.Value);
            }

            var transactions = await query.ToListAsync(cancellationToken);

            // Calculate total revenue
            var totalRevenue = transactions.Sum(t => t.Amount);

            // Group transactions by time period based on selectBy parameter
            var stats = new List<RevenueStatDto>();

            switch (request.SelectBy.ToLower())
            {
                case "month":
                    stats = transactions
                        .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                        .Select(g => new RevenueStatDto
                        {
                            Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Revenue = g.Sum(t => t.Amount)
                        })
                        .OrderBy(s => s.Period)
                        .ToList();
                    break;

                case "quarter":
                    stats = transactions
                        .GroupBy(t => new { t.CreatedAt.Year, Quarter = (t.CreatedAt.Month - 1) / 3 + 1 })
                        .Select(g => new RevenueStatDto
                        {
                            Period = $"{g.Key.Year}-Q{g.Key.Quarter}",
                            Revenue = g.Sum(t => t.Amount)
                        })
                        .OrderBy(s => s.Period)
                        .ToList();
                    break;

                case "year":
                    stats = transactions
                        .GroupBy(t => t.CreatedAt.Year)
                        .Select(g => new RevenueStatDto
                        {
                            Period = g.Key.ToString(),
                            Revenue = g.Sum(t => t.Amount)
                        })
                        .OrderBy(s => s.Period)
                        .ToList();
                    break;

                default:
                    // Default to monthly
                    stats = transactions
                        .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                        .Select(g => new RevenueStatDto
                        {
                            Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Revenue = g.Sum(t => t.Amount)
                        })
                        .OrderBy(s => s.Period)
                        .ToList();
                    break;
            }

            return new CoachRevenueReportDto
            {
                TotalRevenue = totalRevenue,
                Stats = stats,
                DateRange = new DateRangeDto
                {
                    StartDate = startDate?.ToString("yyyy-MM-dd"),
                    EndDate = endDate?.ToString("yyyy-MM-dd")
                }
            };
        }
    }
}
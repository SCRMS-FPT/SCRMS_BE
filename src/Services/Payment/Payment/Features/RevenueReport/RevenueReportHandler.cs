using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Data.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.API.Features.RevenueReport
{
    public record GetRevenueReportQuery(string? StartDate, string? EndDate) : IRequest<RevenueReportDto>;

    public class RevenueReportDto
    {
        public decimal TotalServicePackageRevenue { get; set; }
        public int TotalServicePackagePurchases { get; set; }
        public int TotalServicePackageUsers { get; set; }
        public DateRangeDto DateRange { get; set; }
    }

    public class DateRangeDto
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }

    public class GetRevenueReportHandler : IRequestHandler<GetRevenueReportQuery, RevenueReportDto>
    {
        private readonly PaymentDbContext _context;

        public GetRevenueReportHandler(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<RevenueReportDto> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
        {
            // Parse tham số ngày nếu có
            DateTime? startDate = null, endDate = null;
            if (!string.IsNullOrEmpty(request.StartDate) && DateTime.TryParse(request.StartDate, out var parsedStart))
            {
                startDate = parsedStart;
            }
            if (!string.IsNullOrEmpty(request.EndDate) && DateTime.TryParse(request.EndDate, out var parsedEnd))
            {
                endDate = parsedEnd;
            }

            // Truy vấn các giao dịch mua service package (giả sử PaymentType = "ServicePackage")
            var query = _context.WalletTransactions.AsQueryable()
                .Where(t => t.TransactionType == "ServicePackage");

            if (startDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt.Date <= endDate.Value.Date);
            }

            var transactions = await query.ToListAsync(cancellationToken);

            // Tính toán số liệu
            var totalRevenue = transactions.Sum(t => t.Amount);
            var totalPurchases = transactions.Count;
            var totalUsers = transactions.Select(t => t.UserId).Distinct().Count();

            return new RevenueReportDto
            {
                TotalServicePackageRevenue = totalRevenue,
                TotalServicePackagePurchases = totalPurchases,
                TotalServicePackageUsers = totalUsers,
                DateRange = new DateRangeDto
                {
                    StartDate = startDate?.ToString("yyyy-MM-dd"),
                    EndDate = endDate?.ToString("yyyy-MM-dd")
                }
            };
        }
    }
}
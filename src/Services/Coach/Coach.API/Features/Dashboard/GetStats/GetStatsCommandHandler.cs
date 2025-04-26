using Coach.API.Data;
using Coach.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Coach.API.Features.Dashboard.GetStats
{
    public record GetStatsCommand(
    Guid CoachId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string GroupBy) : IRequest<GetStatsResponse>;

    public record GetStatsResponse(
        int TotalStudents,
        int TotalSessions,
        decimal TotalRevenue,
        int TotalPackage,
        List<StatPeriod> Stats);

    public record StatPeriod(
        string Period,
        int Sessions,
        decimal Revenue);

    public class GetStatsCommandHandler : IRequestHandler<GetStatsCommand, GetStatsResponse>
    {
        private readonly CoachDbContext _context;

        public GetStatsCommandHandler(CoachDbContext context)
        {
            _context = context;
        }

        public async Task<GetStatsResponse> Handle(GetStatsCommand command, CancellationToken cancellationToken)
        {
            // Truy vấn bookings của coach
            var bookingsQuery = _context.CoachBookings
                .Where(b => b.CoachId == command.CoachId);

            // Lọc theo ngày nếu có
            if (command.StartDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= command.StartDate.Value);
            if (command.EndDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= command.EndDate.Value);

            // Tính các số liệu tổng hợp
            var totalStudents = await _context.CoachBookings
                .Where(b => b.CoachId == command.CoachId)
                .Select(b => b.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            var totalSessions = await bookingsQuery
                .CountAsync(b => b.Status == "Completed", cancellationToken);

            var totalRevenue = await bookingsQuery
                .SumAsync(b => b.TotalPrice, cancellationToken);

            var totalPackages = await _context.CoachPackages
                .Where(p => p.CoachId == command.CoachId)
                .CountAsync(cancellationToken);

            // Nhóm dữ liệu theo group_by
            var stats = await GetGroupedStats(bookingsQuery, command.GroupBy, cancellationToken);

            return new GetStatsResponse(totalStudents, totalSessions, totalRevenue, totalPackages, stats);
        }

        private async Task<List<StatPeriod>> GetGroupedStats(IQueryable<CoachBooking> bookingsQuery, string groupBy, CancellationToken cancellationToken)
        {
            var bookings = await bookingsQuery
                .Where(b => b.Status == "completed")
                .Select(b => new { b.BookingDate, b.TotalPrice })
                .ToListAsync(cancellationToken);

            var groupedStats = bookings
                .GroupBy(b => GetPeriodKey(b.BookingDate, groupBy))
                .Select(g => new StatPeriod(
                    Period: g.Key,
                    Sessions: g.Count(),
                    Revenue: g.Sum(b => b.TotalPrice)))
                .OrderBy(s => s.Period)
                .ToList();

            return groupedStats;
        }

        private string GetPeriodKey(DateOnly date, string groupBy)
        {
            return groupBy.ToLower() switch
            {
                "day" => date.ToString("yyyy-MM-dd"),
                "week" => $"{date.Year}-W{CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday):D2}",
                "month" => date.ToString("yyyy-MM"),
                "year" => date.ToString("yyyy"),
                _ => throw new ArgumentException("Invalid group_by value")
            };
        }
    }
}
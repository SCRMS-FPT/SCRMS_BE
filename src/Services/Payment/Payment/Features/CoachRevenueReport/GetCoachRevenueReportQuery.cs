namespace Payment.API.Features.CoachRevenueReport
{
    public record GetCoachRevenueReportQuery(
        Guid CoachId,
        string? StartDate,
        string? EndDate,
        string SelectBy = "month") : IRequest<CoachRevenueReportDto>;

    public class CoachRevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public List<RevenueStatDto> Stats { get; set; } = new();
        public DateRangeDto DateRange { get; set; } = new();
    }

    public class RevenueStatDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class DateRangeDto
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
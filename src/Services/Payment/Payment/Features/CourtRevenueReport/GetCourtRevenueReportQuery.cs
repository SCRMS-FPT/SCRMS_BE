using MediatR;

namespace Payment.API.Features.CourtRevenueReport
{
    public record GetCourtRevenueReportQuery(
        Guid CourtOwnerId,
        string? StartDate,
        string? EndDate,
        string SelectBy = "month") : IRequest<CourtRevenueReportDto>;

    public class CourtRevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public List<CourtRevenueStatDto> Stats { get; set; } = new();
        public CourtDateRangeDto DateRange { get; set; } = new();
    }

    public class CourtRevenueStatDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class CourtDateRangeDto
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
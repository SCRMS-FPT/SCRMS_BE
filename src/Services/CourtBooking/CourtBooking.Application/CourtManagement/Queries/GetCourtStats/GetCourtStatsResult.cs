namespace CourtBooking.Application.CourtManagement.Queries.GetCourtStats
{
    public class DateRange
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }

    public class GetCourtStatsResult
    {
        public long TotalCourts { get; set; }
        public decimal TotalCourtsRevenue { get; set; }
        public DateRange DateRange { get; set; } = new DateRange();
    }
} 
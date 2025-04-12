using MediatR;

namespace Reviews.API.Features.GetReviewStats
{
    public class GetReviewStatsQuery : IRequest<GetReviewStatsResponse>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public GetReviewStatsQuery()
        {
        }

        public GetReviewStatsQuery(DateTime? startDate, DateTime? endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
    }

    public class DateRangeDto
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }

    public class GetReviewStatsResponse
    {
        public int TotalReviews { get; set; }
        public int ReportedReviews { get; set; }
        public DateRangeDto DateRange { get; set; } = new DateRangeDto();
    }
}
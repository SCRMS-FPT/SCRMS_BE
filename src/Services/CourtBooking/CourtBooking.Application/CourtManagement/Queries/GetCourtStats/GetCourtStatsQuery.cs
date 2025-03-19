using MediatR;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtStats
{
    public record GetCourtStatsQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<GetCourtStatsResult>;
} 
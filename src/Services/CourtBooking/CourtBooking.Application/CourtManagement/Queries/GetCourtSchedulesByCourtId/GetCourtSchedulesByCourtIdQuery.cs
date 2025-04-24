using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;

public record GetCourtSchedulesByCourtIdQuery : IQuery<GetCourtSchedulesByCourtIdResult>
{
    public Guid CourtId { get; }
    public int? Day { get; }

    public GetCourtSchedulesByCourtIdQuery(Guid courtId, int? day = null)
    {
        if (courtId == Guid.Empty)
            throw new ArgumentException("CourtId không được rỗng", nameof(courtId));

        CourtId = courtId;
        Day = day;
    }
}

public record GetCourtSchedulesByCourtIdResult(List<CourtScheduleDTO> CourtSchedules);
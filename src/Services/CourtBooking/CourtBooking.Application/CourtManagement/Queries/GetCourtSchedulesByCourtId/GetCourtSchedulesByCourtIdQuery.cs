using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;

public record GetCourtSchedulesByCourtIdQuery(
    Guid CourtId,
    int? Day = null
) : IQuery<GetCourtSchedulesByCourtIdResult>;

public record GetCourtSchedulesByCourtIdResult(List<CourtScheduleDTO> CourtSchedules);
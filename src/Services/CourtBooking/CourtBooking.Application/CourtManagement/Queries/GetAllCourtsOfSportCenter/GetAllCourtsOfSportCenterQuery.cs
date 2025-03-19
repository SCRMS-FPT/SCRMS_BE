using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetAllCourtsOfSportCenter;

public record GetAllCourtsOfSportCenterQuery(Guid SportCenterId) : IQuery<GetAllCourtsOfSportCenterResult>;

public record GetAllCourtsOfSportCenterResult(List<CourtDTO> Courts);
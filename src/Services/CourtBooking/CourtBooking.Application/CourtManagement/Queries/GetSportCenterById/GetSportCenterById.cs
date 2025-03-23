using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetSportCenterById;

public record GetSportCenterByIdQuery(Guid Id) : IQuery<GetSportCenterByIdResult>;

public record GetSportCenterByIdResult(SportCenterDetailDTO SportCenter);
using CourtBooking.Application.DTOs;

    public record GetAllCourtsOfSportCenterQuery(Guid SportCenterId) : IQuery<GetAllCourtsOfSportCenterResult>;

    public record GetAllCourtsOfSportCenterResult(List<CourtDTO> Courts);


using CourtBooking.Application.DTOs;

public record UpdateSportCenterCommand(
    Guid SportCenterId,
    string Name,
    string PhoneNumber,
    string Description,
    LocationDTO Location,
    GeoLocation LocationPoint,
    SportCenterImages Images
) : ICommand<UpdateSportCenterResult>;

public record UpdateSportCenterResult(bool Success);

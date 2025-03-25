namespace CourtBooking.Application.DTOs;

public record SportCenterListDTO(
    Guid Id,
    string Name,
    string PhoneNumber,
    List<string> SportNames,
    string Address,
    string Description,
    string Avatar,
    List<string> ImageUrl,
    List<CourtListDTO> Courts
);
public record CourtListDTO(
    Guid Id,
    string Name,
    Guid SportId,
    string SportName,
    bool IsActive,
    string Description,
    decimal? MinDepositPercentage
);
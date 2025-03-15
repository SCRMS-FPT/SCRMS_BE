namespace CourtBooking.Application.DTOs;

public record SportCenterListDTO(
    Guid Id,
    Guid OwnerId,
    string Name,
    string PhoneNumber,
    string AddressLine,
    string City,
    string District,
    string Commune,
    double Latitude,
    double Longitude,
    string Avatar,
    List<string> ImageUrls,
    string Description,
    DateTime CreatedAt,
    DateTime? LastModified
);
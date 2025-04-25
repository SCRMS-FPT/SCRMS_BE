using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record SportCenterDetailDTO(
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
    bool IsDeleted,
    string Description,
    DateTime CreatedAt,
    DateTime? LastModified
);
}

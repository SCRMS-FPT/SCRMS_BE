using CourtBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record CourtDTO(
    Guid Id,
    string CourtName,
    Guid SportId,
    Guid SportCenterId,
    string Description,
    List<FacilityDTO>? Facilities,
    TimeSpan SlotDuration,
    CourtStatus Status,
    CourtType CourtType,
    string? SportName,
    string? SportCenterName,
    DateTime CreatedAt,
    DateTime? LastModified
    );
}

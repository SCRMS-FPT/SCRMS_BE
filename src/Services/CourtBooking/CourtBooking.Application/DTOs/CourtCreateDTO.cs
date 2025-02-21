using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record CourtCreateDTO
    (
        string CourtName,
        Guid SportId,
        string Description,
        List<FacilityDTO> Facilities,
        decimal PricePerHour,
        LocationDTO Address,
        Guid OwnerId,
        List<CourtOperatingHourDTO> OperatingHours
     );
}

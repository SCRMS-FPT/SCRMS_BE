using CourtBooking.Domain.Enums;
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
        Guid SportCenterId,
        Guid SportId,
        string Description,
        List<FacilityDTO> Facilities,
        int CourtType,
        TimeSpan SlotDuration,
        List<CourtScheduleDTO> CourtSchedules
     );
}

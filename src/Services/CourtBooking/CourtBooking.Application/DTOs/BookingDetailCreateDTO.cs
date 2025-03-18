using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record BookingDetailCreateDTO(
        Guid CourtId,
        TimeSpan StartTime,
        TimeSpan EndTime
        );
}

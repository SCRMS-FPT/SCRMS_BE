using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record CourtScheduleDTO
    (
        Guid CourtId,
        int[] DayOfWeek, 
        TimeSpan StartTime, 
        TimeSpan EndTime, 
        decimal PriceSlot,
        int Status
    );
}

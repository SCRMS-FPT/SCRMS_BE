﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record CourtSlotDTO
    (DateTime slotDate, int[] dayOfWeek, TimeSpan startTime, TimeSpan endTime, decimal priceSlot);
}

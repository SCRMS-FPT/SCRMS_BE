﻿using CourtBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record CourtDTO
    (
        string CourtName, 
        string Description,
        SportId SportId,
        TimeSpan SlotDuration,
        string Facilities,
        CourtStatus Status
        //List<CourtSlotDTO> CourtSlots
    );
}

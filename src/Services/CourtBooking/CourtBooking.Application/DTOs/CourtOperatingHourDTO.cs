﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record CourtOperatingHourDTO
    (string Day, string OpenTime, string CloseTime);
 
}

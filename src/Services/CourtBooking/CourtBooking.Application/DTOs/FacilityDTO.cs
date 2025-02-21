using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record FacilityDTO
    {
        public string Name { get; init; }
        public string Description { get; init; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record LocationDTO(string Address, string Commune, string District, string City);
}

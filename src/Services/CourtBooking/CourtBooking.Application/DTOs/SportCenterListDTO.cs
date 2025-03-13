using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record SportCenterListDTO(
    Guid Id,
    string Name,
    string PhoneNumber,
    List<string> SportNames,
    string Address,
    string Description,
    string ImageUrl

    );
}

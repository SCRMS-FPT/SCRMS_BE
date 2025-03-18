using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
    public record SportCenterDTO(
    Guid Id,
    string Name,
    string PhoneNumber,
    string Address,
    string Description,
    List<CourtDTO> Courts
    );
}
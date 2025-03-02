using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs
{
   public record CourtDTO
    (
        Guid Id,
        string CourtName, 
        string Description
        //Guid OwnerId, 
        //decimal Price
    );
}

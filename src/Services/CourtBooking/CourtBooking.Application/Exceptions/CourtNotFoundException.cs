using BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.Exceptions
{
    public class CourtNotFoundException : NotFoundException
    {
        public CourtNotFoundException(Guid courtId) : base($"Court with id {courtId} was not found.")
        {
        }
    }
}

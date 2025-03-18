using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.BookingManagement.Queries.GetBookingById
{
    public record GetBookingByIdQuery(Guid BookingId, Guid UserId, string Role) : IRequest<BookingDto?>;
}
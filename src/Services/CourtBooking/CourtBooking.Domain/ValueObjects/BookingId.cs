using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{

    public record BookingId
    {
        public Guid Value { get; }
        public BookingId(Guid value) => Value = value;
        public static BookingId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("BookingId cannot be empty.");
            }

            return new BookingId(value);
        }
    }
}

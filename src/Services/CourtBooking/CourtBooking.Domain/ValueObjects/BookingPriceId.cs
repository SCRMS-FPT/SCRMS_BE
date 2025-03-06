using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{

    public record BookingPriceId
    {
        public Guid Value { get; }
        public BookingPriceId(Guid value) => Value = value;
        public static BookingPriceId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("BookingPriceId cannot be empty.");
            }

            return new BookingPriceId(value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{

    public record BookingDetailId
    {
        public Guid Value { get; }
        public BookingDetailId(Guid value) => Value = value;
        public static BookingDetailId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("BookingDetailId cannot be empty.");
            }

            return new BookingDetailId(value);
        }
    }
}

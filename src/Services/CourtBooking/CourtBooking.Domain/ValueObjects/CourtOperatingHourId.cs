using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{

    public record CourtOperatingHourId
    {
        public Guid Value { get; }
        public CourtOperatingHourId(Guid value) => Value = value;
        public static CourtOperatingHourId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("CourtOperatingHourId cannot be empty.");
            }
            return new CourtOperatingHourId(value);
        }
    }
}

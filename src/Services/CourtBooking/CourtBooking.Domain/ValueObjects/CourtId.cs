using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{

    public record CourtId
    {
        public Guid Value { get; }
        public CourtId(Guid value) => Value = value;
        public static CourtId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("CourtId cannot be empty.");
            }

            return new CourtId(value);
        }
    }
}

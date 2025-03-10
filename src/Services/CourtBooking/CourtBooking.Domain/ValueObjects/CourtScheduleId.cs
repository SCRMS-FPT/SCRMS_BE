using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{

    public record CourtScheduleId
    {
        public Guid Value { get; }
        public CourtScheduleId(Guid value) => Value = value;
        public static CourtScheduleId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("CourtScheduleId cannot be empty.");
            }
            return new CourtScheduleId(value);
        }
    }
}

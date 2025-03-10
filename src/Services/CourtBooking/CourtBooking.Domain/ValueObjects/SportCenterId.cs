using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{
    public record SportCenterId
    {
        public Guid Value { get; }
        public SportCenterId(Guid value) => Value = value;
        public static SportCenterId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("Sport Center Id cannot be empty.");
            }

            return new SportCenterId(value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourtBooking.Domain.Exceptions;

namespace CourtBooking.Domain.ValueObjects
{
    public record SportId
    {
        public Guid Value { get; }
        public SportId(Guid value) => Value = value;
        public static SportId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("SportId cannot be empty.");
            }
            return new SportId(value);
        }
    }
}

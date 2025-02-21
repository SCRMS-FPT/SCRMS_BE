using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{
    public record OwnerId
    {
        public Guid Value { get; }
        public OwnerId(Guid value) => Value = value;
        public static OwnerId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("OwnerId cannot be empty.");
            }
            return new OwnerId(value);
        }
    }
}

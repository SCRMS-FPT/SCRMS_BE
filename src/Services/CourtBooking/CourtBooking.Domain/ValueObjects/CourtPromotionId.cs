using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{

    public record CourtPromotionId
    {
        public Guid Value { get; }
        public CourtPromotionId(Guid value) => Value = value;
        public static CourtPromotionId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("CourtPromotionId cannot be empty.");
            }

            return new CourtPromotionId(value);
        }
    }
}

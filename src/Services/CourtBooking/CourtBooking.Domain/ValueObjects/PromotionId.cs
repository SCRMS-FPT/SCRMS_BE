using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{

    public record PromotionId
    {
        public Guid Value { get; }
        public PromotionId(Guid value) => Value = value;
        public static PromotionId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("PromotionId cannot be empty.");
            }

            return new PromotionId(value);
        }
    }
}

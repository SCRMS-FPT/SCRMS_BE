using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{
    public class CourtName
    {
        public string Value { get; }
        public CourtName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Court name cannot be empty", nameof(value));
            }
            Value = value;
        }
        public static CourtName Of(string value)
        {
            return new CourtName(value);
        }
        public static implicit operator string(CourtName courtName)
        {
            return courtName.Value;
        }
        public static implicit operator CourtName(string value)
        {
            return new CourtName(value);
        }
    }
}

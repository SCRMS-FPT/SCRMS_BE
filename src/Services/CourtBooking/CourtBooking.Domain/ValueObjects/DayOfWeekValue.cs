using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{
    public class DayOfWeekValue
    {
        public IReadOnlyList<int> Days { get; }

        public DayOfWeekValue(IEnumerable<int> days)
        {
            if (days == null)
            {
                throw new ArgumentNullException(nameof(days), "DayOfWeekValue cannot be null.");
            }
            var dayList = days.Distinct().ToList();
            if (dayList.Count == 0 || dayList.Count > 7)
                throw new DomainException("Invalid day of week count.");
            if (dayList.Any(d => d < 1 || d > 7))
                throw new DomainException("Invalid day of week value.");

            Days = dayList;
        }
        //return
        public static DayOfWeekValue Of(IEnumerable<int> days)
        {
            ArgumentNullException.ThrowIfNull(days);
            return new DayOfWeekValue(days);
        }
    }
}

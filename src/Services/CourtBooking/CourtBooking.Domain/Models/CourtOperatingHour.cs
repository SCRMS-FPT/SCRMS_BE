using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Domain.Models
{
    public class CourtOperatingHour : Entity<CourtOperatingHourId>
    {
        public CourtId CourtId { get; private set; }
        public int DayOfWeek { get; private set; }
        public TimeSpan OpenTime { get; private set; }
        public TimeSpan CloseTime { get; private set; }

        public CourtOperatingHour() { } // For EF Core
        public CourtOperatingHour(CourtId courtId, int dayOfWeek, TimeSpan openTime, TimeSpan closeTime)
        {
            Id = CourtOperatingHourId.Of(Guid.NewGuid());
            CourtId = courtId;
            if (dayOfWeek < 1 || dayOfWeek > 7)
                throw new DomainException("Invalid day of week");
            DayOfWeek = dayOfWeek;
            OpenTime = openTime;
            CloseTime = closeTime;
        }
        public static CourtOperatingHour Create(CourtOperatingHourId courtOperatingHourId, CourtId courtId,
            TimeSpan openTime, TimeSpan closeTime)
        {
            var courtOperatingHour = new CourtOperatingHour
            {
                Id = courtOperatingHourId,
                CourtId = courtId,
                DayOfWeek = 1,
                OpenTime = openTime,
                CloseTime = closeTime
            };
            return courtOperatingHour;
        }
        public static CourtOperatingHour Of(CourtId courtId, int dayOfWeek, TimeSpan openTime, TimeSpan closeTime)
        {
            return new CourtOperatingHour(courtId, dayOfWeek, openTime, closeTime);
        }
    }
}

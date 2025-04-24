using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Domain.Models
{
    public class CourtSchedule : Entity<CourtScheduleId>
    {
        public CourtId CourtId { get; private set; }
        public DayOfWeekValue DayOfWeek { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public decimal PriceSlot { get; private set; }
        public CourtScheduleStatus Status { get; private set; }

        public CourtSchedule() { } // For EF Core

        //public CourtSchedule(CourtScheduleId id, CourtId courtId, DayOfWeekValue dayOfWeek,
        //             TimeSpan startTime, TimeSpan endTime, decimal priceSlot)
        //{
        //    Id = id;
        //    CourtId = courtId ?? throw new DomainException("CourtId is required");
        //    DayOfWeek = dayOfWeek ?? throw new DomainException("DayOfWeek is required");
        //    if (startTime <= endTime)
        //        throw new DomainException("StartTime must be before EndTime");
        //    if (priceSlot < 0)
        //        throw new DomainException("Price must be non-negative");

        //    StartTime = startTime;
        //    EndTime = endTime;
        //    PriceSlot = priceSlot;
        //    Status = CourtSlotStatus.Available;
        //}
        public static CourtSchedule Create(CourtScheduleId courtSlotId, CourtId courtId,
            DayOfWeekValue dayOfWeek, TimeSpan startTime, TimeSpan endTime, decimal priceSlot)
        {
            if (startTime >= endTime)
                throw new DomainException("StartTime must be before EndTime");
            if (priceSlot < 0)
                throw new DomainException("Price must be non-negative");

            return new CourtSchedule
            {
                Id = courtSlotId,
                CourtId = courtId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                PriceSlot = priceSlot,
                Status = CourtScheduleStatus.Available
            };
        }

        public void Update(DayOfWeekValue dayOfWeek, TimeSpan startTime, TimeSpan endTime, decimal priceSlot, CourtScheduleStatus status)
        {
            if (startTime >= endTime)
                throw new DomainException("StartTime must be before EndTime");
            if (priceSlot < 0)
                throw new DomainException("Price must be non-negative");

            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            PriceSlot = priceSlot;
            Status = status;
            SetLastModified(DateTime.UtcNow);
        }
    }
}

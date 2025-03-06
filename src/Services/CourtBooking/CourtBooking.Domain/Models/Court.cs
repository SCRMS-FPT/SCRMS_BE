using CourtBooking.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CourtBooking.Domain.Models
{
    public class Court : Entity<CourtId>
    {
        public CourtName CourtName { get; private set; }
        public SportCenterId SportCenterId { get; private set; }
        public SportId SportId { get; private set; }
        public TimeSpan SlotDuration { get; private set; }
        public string? Description { get; private set; }
        public string? Facilities { get; private set; }
        public CourtStatus Status { get; private set; }

        private List<CourtSchedule> _courtSlots = new();
        public IReadOnlyCollection<CourtSchedule> CourtSlots => _courtSlots.AsReadOnly();


        public static Court Create(CourtId courtId,CourtName courtName, SportCenterId sportCenterId,
            SportId sportId, TimeSpan slotDuration, string? description,
            string? facilities)
        {
            var court = new Court
            {
                Id = courtId,
                CourtName = courtName,
                SportCenterId = sportCenterId,
                SportId = sportId,
                SlotDuration = slotDuration,
                Description = description,
                Facilities = facilities,
                Status = CourtStatus.Open,
                CreatedAt = DateTime.UtcNow,
            };
            return court;
        }

        public void UpdateCourt(CourtName courtName,
            SportId sportId, TimeSpan slotDuration, string? description, 
            string? facilities, CourtStatus courtStatus)
        {
            CourtName = courtName;
            SportId = sportId;
            SlotDuration = slotDuration;
            Description = description;
            Facilities = facilities;
            Status = courtStatus;
            SetLastModified(DateTime.UtcNow);
        }

        public void AddCourtSlot(CourtId courtId,int[] dayOfWeek, TimeSpan startTime, TimeSpan endTime, decimal priceSlot)
        {
            var dayOfWeekValue = new DayOfWeekValue(dayOfWeek);
            var courtSlot =  CourtSchedule.Create(CourtScheduleId.Of(Guid.NewGuid()), courtId,
                dayOfWeekValue, startTime, endTime, priceSlot);
            courtSlot.CreatedAt = DateTime.UtcNow;
            _courtSlots.Add(courtSlot);
        }
    }

}

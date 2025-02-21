using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Domain.Models
{
    public class Court : Aggregate<CourtId>
    {
        public CourtName CourtName { get; private set; }
        public SportId SportId { get; private set; }
        public Sport Sport { get; }
        public Location Location { get; private set; }
        public string Description { get; private set; }
        public string Facilities { get; private set; }
        public decimal PricePerHour { get; private set; }
        public CourtStatus Status { get; private set; }
        public OwnerId OwnerId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private List<CourtOperatingHour> _operatingHours = new();
        public IReadOnlyCollection<CourtOperatingHour> OperatingHours => _operatingHours.AsReadOnly();

        private Court() { } // For EF Core

        public Court(CourtId courtId, CourtName courtName, SportId sportId, Location location, string description,
                     string facilities, decimal pricePerHour, OwnerId ownerId)
        {
            Id = courtId;
            CourtName = CourtName;
            SportId = sportId;
            Location = location;
            Description = description;
            Facilities = facilities;
            PricePerHour = pricePerHour;
            Status = CourtStatus.Open;
            OwnerId = ownerId;
            CreatedAt = DateTime.UtcNow;
        }

        public static Court Create(CourtId courtId, CourtName courtName, SportId sportId, Location location, string description,
                                   string facilities, decimal pricePerHour, OwnerId ownerId)
        {
            var court = new Court
            {
                Id = courtId,
                CourtName = courtName,
                SportId = sportId,
                Location = location,
                Description = description,
                Facilities = facilities,
                PricePerHour = pricePerHour,
                Status = CourtStatus.Open,
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow
            };
            return court;
        }

        public void UpdateStatus(CourtStatus newStatus)
        {
            Status = newStatus;
        }

        public void AddOperatingHour(CourtOperatingHour hour)
        {
            _operatingHours.Add(hour);
        }
        public bool IsAvailable(DateTime dateTime)
        {
            int dayOfWeek = (int)dateTime.DayOfWeek;
            var operatingHour = _operatingHours.FirstOrDefault(oh => oh.DayOfWeek == dayOfWeek);
            if (operatingHour == null)
                return false;

            TimeSpan timeOfDay = dateTime.TimeOfDay;
            return timeOfDay >= operatingHour.OpenTime &&
                   timeOfDay <= operatingHour.CloseTime &&
                   Status == CourtStatus.Open;
        }

        public void UpdateCourt(CourtName courtName, SportId sportId, Location location, string description,
                            string facilities, decimal pricePerHour, CourtStatus status)
        {
            CourtName = courtName;
            SportId = sportId;
            Location = location;
            Description = description;
            Facilities = facilities;
            PricePerHour = pricePerHour;
            Status = CourtStatus.Open;
        }
    }

}

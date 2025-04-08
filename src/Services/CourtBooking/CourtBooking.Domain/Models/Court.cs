using CourtBooking.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Nodes;
using CourtBooking.Domain.Enums;

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
        public CourtType CourtType { get; private set; }
        public decimal MinDepositPercentage { get; private set; } = 100;

        public int CancellationWindowHours { get; private set; } = 24;
        public decimal RefundPercentage { get; private set; } = 0;

        private List<CourtSchedule> _courtSchedules = new();
        public IReadOnlyCollection<CourtSchedule> CourtSchedules => _courtSchedules.AsReadOnly();

        public static Court Create(CourtId courtId, CourtName courtName,
            SportCenterId sportCenterId, SportId sportId,
            TimeSpan slotDuration, string? description, string? facilities,
            CourtType courtType, decimal minDepositPercentage = 100,
            int CancellationWindowHours = 24, decimal RefundPercentage = 0)
        {
            if (minDepositPercentage < 0 || minDepositPercentage > 100)
                throw new DomainException("Tỷ lệ đặt cọc phải nằm trong khoảng từ 0 đến 100");

            return new Court
            {
                Id = courtId,
                CourtName = courtName,
                SportCenterId = sportCenterId,
                SportId = sportId,
                SlotDuration = slotDuration,
                Description = description,
                Facilities = facilities,
                Status = CourtStatus.Open,
                CourtType = courtType,
                MinDepositPercentage = minDepositPercentage,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateCourt(CourtName courtName, SportId sportId, TimeSpan slotDuration, string? description,
            string? facilities, CourtStatus courtStatus, CourtType courtType, decimal minDepositPercentage = 100, int cancellationWindowHours = 24, decimal refundPercentage = 0)
        {
            if (minDepositPercentage < 0 || minDepositPercentage > 100)
                throw new DomainException("Tỷ lệ đặt cọc phải nằm trong khoảng từ 0 đến 100");

            CourtName = courtName;
            SportId = sportId;
            SlotDuration = slotDuration;
            Description = description;
            Facilities = facilities;
            Status = courtStatus;
            CourtType = courtType;
            MinDepositPercentage = minDepositPercentage;
            SetLastModified(DateTime.UtcNow);
            CancellationWindowHours = cancellationWindowHours;
            RefundPercentage = refundPercentage;
        }

        public void UpdateCancellationPolicy(int cancellationWindowHours, decimal refundPercentage)
        {
            CancellationWindowHours = cancellationWindowHours;
            RefundPercentage = refundPercentage;
        }
        public void UpdateStatus(CourtStatus status)
        {
            Status = status;
            SetLastModified(DateTime.UtcNow);
        }
        public void AddCourtSlot(CourtId courtId, int[] dayOfWeek, TimeSpan startTime, TimeSpan endTime, decimal priceSlot)
        {
            var dayOfWeekValue = new DayOfWeekValue(dayOfWeek);
            var courtSlot = CourtSchedule.Create(CourtScheduleId.Of(Guid.NewGuid()), courtId,
                dayOfWeekValue, startTime, endTime, priceSlot);
            courtSlot.CreatedAt = DateTime.UtcNow;
            _courtSchedules.Add(courtSlot);
        }
    }
}
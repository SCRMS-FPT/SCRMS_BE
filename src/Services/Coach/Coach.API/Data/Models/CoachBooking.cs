using System.ComponentModel.DataAnnotations;

namespace Coach.API.Data.Models
{
    public class CoachBooking
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid CoachId { get; set; }
        public Guid SportId { get; set; }
        public DateOnly BookingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; } = "pending";
        public decimal TotalPrice { get; set; }
        public Guid? PackageId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Coach Coach { get; set; }
        public virtual CoachPackage Package { get; set; }
    }
}
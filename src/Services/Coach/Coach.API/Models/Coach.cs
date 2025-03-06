using System.ComponentModel.DataAnnotations;

namespace Coach.API.Models
{
    public class Coach
    {
        [Key]
        public Guid UserId { get; set; }

        public string Bio { get; set; } = string.Empty;
        public decimal RatePerHour { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CoachSchedule> Schedules { get; set; }
        public virtual ICollection<CoachSport> CoachSports { get; set; }
        public virtual ICollection<CoachBooking> Bookings { get; set; }
        public virtual ICollection<CoachPackage> Packages { get; set; }
        public virtual ICollection<CoachPromotion> Promotions { get; set; }
    }
}
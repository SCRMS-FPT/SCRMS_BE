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

        public ICollection<CoachSchedule> Schedules { get; set; }
        public ICollection<CoachSport> CoachSports { get; set; }
        public ICollection<CoachBooking> Bookings { get; set; }
        public ICollection<CoachPackage> Packages { get; set; }
    }
}
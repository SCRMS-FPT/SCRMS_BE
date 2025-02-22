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

        public List<CoachSport> Sports { get; set; } = new();
        public List<CoachSchedule> Schedules { get; set; } = new();
        public List<CoachPackage> Packages { get; set; } = new();
    }
}
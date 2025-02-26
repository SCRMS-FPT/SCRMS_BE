using System.ComponentModel.DataAnnotations;

namespace Coach.API.Models
{
    public class CoachSchedule
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CoachId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Coach Coach { get; set; }
    }
}
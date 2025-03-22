using System.ComponentModel.DataAnnotations;

namespace Coach.API.Data.Models
{
    public class CoachSchedule
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CoachId { get; set; }

        [Required]
        public int DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual Coach Coach { get; set; }
    }
}
namespace Coach.API.Models
{
    public class CoachSport
    {
        public Guid CoachId { get; set; }
        public Guid SportId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Coach Coach { get; set; } = null!;
    }
}
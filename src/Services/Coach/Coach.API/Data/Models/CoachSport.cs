namespace Coach.API.Data.Models
{
    public class CoachSport
    {
        public Guid CoachId { get; set; }
        public Guid SportId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Coach Coach { get; set; } = null!;
    }
}
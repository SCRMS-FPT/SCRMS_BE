namespace Coach.API.Models
{
    public class CoachBooking
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CoachId { get; set; }
        public int SportId { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
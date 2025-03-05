namespace Identity.Domain.Models
{
    public class CoachServicePromotion
    {
        public Guid Id { get; set; }
        public Guid CoachId { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
 
    }
}

using System.ComponentModel.DataAnnotations;

namespace Coach.API.Models
{
    public class CoachPackage
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CoachId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SessionCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}